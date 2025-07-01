using Microsoft.AspNetCore.Identity;

namespace WaterJarAttendanceSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int RatePerDay { get; set; }  // Store parsed int value here
        public string Name { get; set; } // Add this line

    }
}
