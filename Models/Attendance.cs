using System.ComponentModel.DataAnnotations;
namespace WaterJarAttendanceSystem.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } // "Present" or "Absent"
    }
}