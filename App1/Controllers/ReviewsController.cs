using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App1.Models;
using App1.Services;
using System.Security.Claims;
using App1.Data;

namespace App1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewService _reviewService;
        private readonly ApplicationDbContext _context;

        public ReviewsController(ReviewService reviewService, ApplicationDbContext context)
        {
            _reviewService = reviewService;
            _context = context;
        }

        // GET: api/reviews/course/5
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<List<ReviewResponse>>> GetCourseReviews(int courseId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByCourseIdAsync(courseId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/reviews/course/5
        [Authorize]
        [HttpPost("course/{courseId}")]
        public async Task<ActionResult> CreateReview(int courseId, CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Получаем ID пользователя из токена
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "Неверный токен авторизации" });
                }

                // Проверяем, существует ли курс
                var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
                if (!courseExists)
                {
                    return NotFound(new { error = "Курс не найден" });
                }

                await _reviewService.CreateReviewAsync(userId, courseId, request);
                return Ok(new { message = "Отзыв успешно добавлен" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/reviews/course/5/stats
        [HttpGet("course/{courseId}/stats")]
        public async Task<ActionResult> GetCourseStats(int courseId)
        {
            try
            {
                var averageRating = await _reviewService.GetAverageRatingAsync(courseId);
                var reviewCount = await _reviewService.GetReviewCountAsync(courseId);

                return Ok(new
                {
                    averageRating,
                    reviewCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}