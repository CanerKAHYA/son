using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using spering_html.Models;
using Microsoft.AspNetCore.Authentication;

namespace spering_html.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Category()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User user)
        {
            if (user.Password != null && user.Email != null)
            {
                var dbuser = _context.Users.FirstOrDefault(x => x.Email == user.Email);
                if (dbuser == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                    return View(user);
                }
                if (dbuser.Password != user.Password)
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz şifre.");
                    return View(user);
                }

                // Kullanıcı bilgilerini session'a ekle
             _httpContextAccessor.HttpContext.Session.SetString("UserEmail", dbuser.Email);
_httpContextAccessor.HttpContext.Session.SetString("Username", dbuser.Username);
             

                return RedirectToAction("Profil");
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Profil");
            }
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(referer);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Profil()
        {
            var users = _context.Users.ToList();
    var usersWithPhotos = _context.Users.Include(u => u.Photos).ToList();
    ViewData["Users"] = users;
    ViewData["UsersWithPhotos"] = usersWithPhotos;

    // Kullanıcı bilgilerini session'dan al
    ViewData["Username"] = HttpContext.Session.GetString("Username");
    ViewData["Email"] = HttpContext.Session.GetString("UserEmail");

    return View(users);
        }

      [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile photo, int userId)
        {
            if (photo != null && photo.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/images", photo.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                var userPhoto = new UserPhoto
                {
                    PhotoPath = $"/images/{photo.FileName}",
                    UserId = userId
                };

                _context.UserPhotos.Add(userPhoto);
                _context.SaveChanges();
            }

            return RedirectToAction("Profil");
        }
        public IActionResult EditProfile()
{
    var userEmail = HttpContext.Session.GetString("UserEmail");
    var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
    if (user == null)
    {
        return RedirectToAction("Error");
    }
    return View(user);
}
 public IActionResult AdminPanel()
    {
        // Sadece Admin rolüne sahip kullanıcılar erişebilir
        if (!User.IsInRole(UserRole.Admin.ToString()))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        // Diğer kullanıcıları getir ve admin panelinde görüntüle
        var users = _context.Users.ToList();
        return View(users);
    }

    // Admin panelindeki bir kullanıcıyı silmek için
    [HttpPost]
    public IActionResult DeleteUser(int userId)
    {
        // Sadece Admin rolüne sahip kullanıcılar erişebilir
        if (!User.IsInRole(UserRole.Admin.ToString()))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var user = _context.Users.Find(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        return RedirectToAction("AdminPanel");
    }
[HttpPost]
public IActionResult UpdateProfile(User updatedUser)
{
    if (ModelState.IsValid)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
        if (user == null)
        {
            return RedirectToAction("Error");
        }
        
        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;
        
        _context.SaveChanges();
        
        return RedirectToAction("Profil");
    }
    
    return RedirectToAction("Error");
}

        [HttpPost]
        public IActionResult DeletePhoto(int photoId)
        {
            var photo = _context.UserPhotos.Find(photoId);
            if (photo != null)
            {
                var filePath = Path.Combine("wwwroot", photo.PhotoPath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.UserPhotos.Remove(photo);
                _context.SaveChanges();
            }

            return RedirectToAction("Profil");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            
           HttpContext.Session.Clear();

            // Diğer çıkış işlemleri burada yapılabilir (örneğin, cookie temizleme)
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Videohbr()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
