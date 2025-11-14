using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using App1.Models; // Добавьте эту строку
using App1.Data;   // Добавьте эту строку

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Lessons)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                c.ImageUrl,
                c.Duration,
                c.Level,
                LessonsCount = c.Lessons.Count,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons.OrderBy(l => l.OrderNumber))
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                c.ImageUrl,
                c.Duration,
                c.Level,
                Lessons = c.Lessons.Select(l => new
                {
                    l.Id,
                    l.Title,
                    l.OrderNumber,
                    l.Duration,
                    Content = l.Content.Length > 100 ? l.Content.Substring(0, 100) + "..." : l.Content
                }),
                c.CreatedAt
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        return Ok(course);
    }

    [HttpGet("{courseId}/lessons/{lessonId}")]
    public async Task<IActionResult> GetLesson(int courseId, int lessonId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .Where(l => l.Id == lessonId && l.CourseId == courseId)
            .Select(l => new
            {
                l.Id,
                l.Title,
                l.Content,
                l.OrderNumber,
                l.Duration,
                Course = new { l.Course.Id, l.Course.Title }
            })
            .FirstOrDefaultAsync();

        if (lesson == null)
            return NotFound();

        return Ok(lesson);
    }

    [Authorize]
    [HttpPost("{courseId}/enroll")]
    public async Task<IActionResult> EnrollInCourse(int courseId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Check if already enrolled
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

        if (existingEnrollment != null)
            return BadRequest(new { message = "Already enrolled in this course" });

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            Progress = 0
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully enrolled in course" });
    }

    [Authorize]
    [HttpPost("course/{courseId}")]
    public async Task<ActionResult> CreateReview(int courseId, CreateReviewRequest request)
    {
        Console.WriteLine("=== START CREATE REVIEW ===");

        if (!ModelState.IsValid)
        {
            Console.WriteLine($"ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
            return BadRequest(ModelState);
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            Console.WriteLine($"UserId claim: {userIdClaim?.Value}");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                Console.WriteLine("Invalid user ID claim");
                return Unauthorized(new { error = "Неверный токен авторизации" });
            }

            Console.WriteLine($"Creating review - UserId: {userId}, CourseId: {courseId}, Rating: {request.Rating}, Content: {request.Content}, LessonId: {request.LessonId}");

            // Проверяем пользователя
            var user = await _context.Users.FindAsync(userId);
            Console.WriteLine($"User exists: {user != null}");

            // Проверяем курс
            var course = await _context.Courses.FindAsync(courseId);
            Console.WriteLine($"Course exists: {course != null}");

            // Создаем отзыв БЕЗ LessonId
            var review = new Review
            {
                UserId = userId,
                CourseId = courseId,
                LessonId = null, // Явно указываем null
                Rating = request.Rating,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            Console.WriteLine("Before Add...");
            _context.Reviews.Add(review);

            Console.WriteLine("Before SaveChanges...");
            await _context.SaveChangesAsync();

            Console.WriteLine($"Review saved successfully with ID: {review.Id}");

            return Ok(new
            {
                message = "Отзыв успешно добавлен",
                reviewId = review.Id
            });
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"DB UPDATE EXCEPTION: {dbEx.Message}");
            Console.WriteLine($"INNER: {dbEx.InnerException?.Message}");
            Console.WriteLine($"INNER INNER: {dbEx.InnerException?.InnerException?.Message}");

            return BadRequest(new
            {
                error = "Database error",
                message = dbEx.InnerException?.Message,
                details = dbEx.InnerException?.InnerException?.Message
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GENERAL EXCEPTION: {ex}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
        finally
        {
            Console.WriteLine("=== END CREATE REVIEW ===");
        }
    }

    [Authorize]
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
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
                TotalLessons = e.Course.Lessons.Count
            })
            .ToListAsync();

        return Ok(enrollments);
    }
}