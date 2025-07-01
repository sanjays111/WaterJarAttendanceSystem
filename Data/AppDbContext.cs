using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaterJarAttendanceSystem.Models;

namespace WaterJarAttendanceSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Attendance> Attendances { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Attendance>()
                   .HasIndex(a => new { a.UserId, a.Date })
                   .IsUnique();
        }
    }
}
