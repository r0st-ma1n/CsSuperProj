using Microsoft.EntityFrameworkCore;
using App1.Models;
using App1.Data;

namespace App1.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получить все отзывы для курса
        public async Task<List<ReviewResponse>> GetReviewsByCourseIdAsync(int courseId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.CourseId == courseId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    UserName = r.User.Email,
                    UserFullName = $"{r.User.FirstName} {r.User.LastName}"
                })
                .ToListAsync();

            return reviews;
        }

        // Создать новый отзыв
        public async Task<Review> CreateReviewAsync(int userId, int courseId, CreateReviewRequest request)
        {
            // Проверяем, существует ли уже отзыв от этого пользователя для этого курса
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId);

            if (existingReview != null)
            {
                throw new Exception("Вы уже оставляли отзыв для этого курса");
            }

            var review = new Review
            {
                UserId = userId,
                CourseId = courseId,
                Rating = request.Rating,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return review;
        }

        // Получить средний рейтинг курса
        public async Task<double> GetAverageRatingAsync(int courseId)
        {
            var average = await _context.Reviews
                .Where(r => r.CourseId == courseId)
                .AverageAsync(r => (double?)r.Rating) ?? 0.0;

            return Math.Round(average, 1);
        }

        // Получить количество отзывов для курса
        public async Task<int> GetReviewCountAsync(int courseId)
        {
            return await _context.Reviews
                .CountAsync(r => r.CourseId == courseId);
        }
    }
}