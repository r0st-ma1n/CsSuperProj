using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.CreatedAt,
                FullName = $"{u.FirstName} {u.LastName}"
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        var enrollments = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
            .ThenInclude(c => c.Lessons)
            .Select(e => new
            {
                Course = new
                {
                    e.Course.Id,
                    e.Course.Title,
                    e.Course.Description,
                    e.Course.ImageUrl,
                    e.Course.Duration,
                    e.Course.Level
                },
                e.EnrolledAt,
                e.Progress,
                CompletedLessons = _context.UserProgresses
                    .Count(up => up.UserId == userId && up.CourseId == e.CourseId && up.IsCompleted),
                TotalLessons = e.Course.Lessons.Count,
                LastActivity = _context.UserProgresses
                    .Where(up => up.UserId == userId && up.CourseId == e.CourseId)
                    .OrderByDescending(up => up.CompletedAt)
                    .Select(up => up.CompletedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var totalCourses = enrollments.Count;
        var totalLessonsCompleted = enrollments.Sum(e => e.CompletedLessons);
        var totalLessons = enrollments.Sum(e => e.TotalLessons);
        var overallProgress = totalLessons > 0 ? (decimal)totalLessonsCompleted / totalLessons * 100 : 0;

        return Ok(new
        {
            User = user,
            Statistics = new
            {
                TotalCourses = totalCourses,
                TotalLessonsCompleted = totalLessonsCompleted,
                TotalLessons = totalLessons,
                OverallProgress = Math.Round(overallProgress, 1)
            },
            Enrollments = enrollments
        });
    }

    [HttpGet("progress/{courseId}")]
    public async Task<IActionResult> GetCourseProgress(int courseId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var progress = await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderNumber)
            .Select(l => new
            {
                Lesson = new
                {
                    l.Id,
                    l.Title,
                    l.OrderNumber,
                    l.Duration
                },
                IsCompleted = _context.UserProgresses
                    .Any(up => up.UserId == userId && up.LessonId == l.Id && up.IsCompleted),
                CompletedAt = _context.UserProgresses
                    .Where(up => up.UserId == userId && up.LessonId == l.Id && up.IsCompleted)
                    .Select(up => up.CompletedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(progress);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully" });
    }
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}