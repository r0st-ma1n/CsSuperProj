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
            // Проверяем, не оставлял ли пользователь уже отзыв для этого курса/урока
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId &&
                                         r.CourseId == courseId &&
                                         r.LessonId == request.LessonId);

            if (existingReview != null)
            {
                throw new InvalidOperationException("Вы уже оставляли отзыв для этого урока");
            }

            var review = new Review
            {
                UserId = userId,
                CourseId = courseId,
                LessonId = request.LessonId,
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

            return await query.AverageAsync(r => (double?)r.Rating);
        }

        public async Task<int> GetReviewCountAsync(int courseId, int? lessonId = null)
        {
            var query = _context.Reviews.Where(r => r.CourseId == courseId);

            if (lessonId.HasValue)
            {
                query = query.Where(r => r.LessonId == lessonId);
            }

            return await query.CountAsync();
        }
    }
}