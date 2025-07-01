using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterJarAttendanceSystem.Data;
using WaterJarAttendanceSystem.Models;

namespace WaterJarAttendanceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Fixed Mark
        [HttpPost("mark")]
        public async Task<IActionResult> Mark([FromQuery] string status)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var nowIndia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
            var todayLocalDate = nowIndia.Date;

            var userAttendances = await _context.Attendances
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var alreadyMarked = userAttendances.Any(a =>
            {
                var localDate = TimeZoneInfo.ConvertTimeFromUtc(a.Date, indiaTimeZone).Date;
                return localDate == todayLocalDate;
            });

            if (alreadyMarked)
                return BadRequest("Already marked today.");

            var attendance = new Attendance
            {
                UserId = userId,
                Date = DateTime.UtcNow,
                Status = status
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Marked successfully", status });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var records = await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return Ok(new { history = records });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            var records = await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Date)
                .ToListAsync();

            var formattedRecords = records.Select(a => new
            {
                Date = TimeZoneInfo.ConvertTimeFromUtc(a.Date, indiaTimeZone).ToString("yyyy-MM-dd"),
                Status = a.Status
            }).ToList();

            int presentDays = records.Count(r => r.Status == "Present");
            int absentDays = records.Count(r => r.Status == "Absent");
            int totalBill = presentDays * user.RatePerDay;

            var summary = records
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return Ok(new
            {
                Email = user.Email,
                RatePerDay = user.RatePerDay,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                TotalBill = totalBill,
                Records = formattedRecords,
                Summary = summary
            });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var totalRecords = await _context.Attendances.Where(a => a.UserId == userId).ToListAsync();
            int presentDays = totalRecords.Count(a => a.Status == "Present");
            int totalDays = totalRecords.Count;
            double attendanceRate = totalDays > 0 ? (double)presentDays / totalDays * 100 : 0;
            int totalBill = presentDays * user.RatePerDay;

            return Ok(new
            {
                presentDays,
                totalBill,
                ratePerDay = user.RatePerDay,
                attendanceRate = Math.Round(attendanceRate, 2)
            });
        }
    }
}
