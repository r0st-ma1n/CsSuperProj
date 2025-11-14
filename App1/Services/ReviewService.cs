using Microsoft.EntityFrameworkCore;
using App1.Data;
using App1.Models;

namespace App1.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewResponse>> GetReviewsByCourseIdAsync(int courseId, int? lessonId = null)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId);

            // Если указан lessonId, фильтруем по уроку
            if (lessonId.HasValue)
            {
                query = query.Where(r => r.LessonId == lessonId);
            }
            else
            {
                // Если lessonId не указан, берем только отзывы на курс (где LessonId == null)
                query = query.Where(r => r.LessonId == null);
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    UserName = r.User.Email,
                    UserFullName = $"{r.User.FirstName} {r.User.LastName}",
                    LessonId = r.LessonId
                })
                .ToListAsync();
        }

        public async Task CreateReviewAsync(int userId, int courseId, CreateReviewRequest request)
        {
            // Для отзывов на курс (без урока) проверяем только UserId и CourseId с LessonId == null
            if (request.LessonId.HasValue)
            {
                // Отзыв на конкретный урок
                var existingLessonReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId &&
                                             r.CourseId == courseId &&
                                             r.LessonId == request.LessonId);

                if (existingLessonReview != null)
                {
                    throw new InvalidOperationException("Вы уже оставляли отзыв для этого урока");
                }
            }
            else
            {
                // Отзыв на весь курс (LessonId == null)
                var existingCourseReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId &&
                                             r.CourseId == courseId &&
                                             r.LessonId == null);

                if (existingCourseReview != null)
                {
                    throw new InvalidOperationException("Вы уже оставляли отзыв для этого курса");
                }
            }

            var review = new Review
            {
                UserId = userId,
                CourseId = courseId,
                LessonId = request.LessonId, // Будет null для отзывов на курс
                Rating = request.Rating,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<double?> GetAverageRatingAsync(int courseId, int? lessonId = null)
        {
            var query = _context.Reviews.Where(r => r.CourseId == courseId);

            if (lessonId.HasValue)
            {
                query = query.Where(r => r.LessonId == lessonId);
            }
            else
            {
                // Для курса берем только отзывы с LessonId == null
                query = query.Where(r => r.LessonId == null);
            }

            return await query.AverageAsync(r => (double?)r.Rating);
        }

        public async Task<int> GetReviewCountAsync(int courseId, int? lessonId = null)
        {
            var query = _context.Reviews.Where(r => r.CourseId == courseId);

            if (lessonId.HasValue)
            {
                query = query.Where(r => r.LessonId == lessonId);
            }
            else
            {
                // Для курса берем только отзывы с LessonId == null
                query = query.Where(r => r.LessonId == null);
            }

            return await query.CountAsync();
        }
    }
}