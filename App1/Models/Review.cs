using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App1.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        // Добавьте это свойство, если отзывы могут быть к конкретному уроку
        public int? LessonId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MinLength(10)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }
    }

    public class CreateReviewRequest
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MinLength(10)]
        public string Content { get; set; } = string.Empty;

        public int? LessonId { get; set; } // Добавьте это
    }

    public class ReviewResponse
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public int? LessonId { get; set; } // Добавьте это
    }
}