using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using App1.Models; // Добавьте эту строку
using App1.Data;   // Добавьте эту строку

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProgressController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserProgressController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("complete-lesson")]
    public async Task<IActionResult> CompleteLesson([FromBody] CompleteLessonRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if already completed
        var existingProgress = await _context.UserProgresses
            .FirstOrDefaultAsync(up => up.UserId == userId && up.LessonId == request.LessonId);

        if (existingProgress != null)
        {
            return Ok(new { message = "Lesson already completed" });
        }

        // Create progress record
        var progress = new UserProgress
        {
            UserId = userId,
            LessonId = request.LessonId,
            CourseId = request.CourseId,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        _context.UserProgresses.Add(progress);

        // Update course progress - ИСПРАВЬ ЭТУ ЧАСТЬ
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId);

        if (enrollment != null)
        {
            var totalLessons = await _context.Lessons.CountAsync(l => l.CourseId == request.CourseId);
            var completedLessons = await _context.UserProgresses
                .CountAsync(up => up.UserId == userId && up.CourseId == request.CourseId && up.IsCompleted);

            // ДОБАВЬ +1 к completedLessons так как мы только что добавили новый прогресс
            completedLessons += 1;

            enrollment.Progress = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;

            Console.WriteLine($"📊 Progress update: {completedLessons}/{totalLessons} = {enrollment.Progress}%");
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Lesson completed successfully" });
    }
    
    [HttpPost("ensure-enrollment")]
    public async Task<IActionResult> EnsureEnrollment([FromBody] EnsureEnrollmentRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Проверяем есть ли уже запись
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == request.CourseId);

        if (existingEnrollment == null)
        {
            // Создаем новую запись
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = request.CourseId,
                EnrolledAt = DateTime.UtcNow,
                Progress = 0
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrollment created" });
        }

        return Ok(new { message = "Enrollment already exists" });
    }

    public class EnsureEnrollmentRequest
    {
        public int CourseId { get; set; }
    }

    [HttpGet("my-progress")]
    public async Task<IActionResult> GetMyProgress()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var progress = await _context.UserProgresses
            .Where(up => up.UserId == userId)
            .Include(up => up.Lesson)
            .Include(up => up.Course)
            .Select(up => new
            {
                up.LessonId,
                up.CourseId,
                CourseTitle = up.Course.Title,
                LessonTitle = up.Lesson.Title,
                up.CompletedAt
            })
            .ToListAsync();

        return Ok(progress);
    }
}

public class CompleteLessonRequest
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
}