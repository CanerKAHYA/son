using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using spering_html.Models;
using Microsoft.EntityFrameworkCore;
namespace spering_html.Models
 {
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
      string baglanti = "Server=(localdb)\\mssqllocaldb;Database=ApplicationDb;Trusted_Connection=True; ";
 
 protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
 {
    optionsBuilder.UseSqlServer(baglanti);
 }
        public DbSet<User> Users { get; set; }
         public DbSet<UserPhoto> UserPhotos { get; set; }
}
}

