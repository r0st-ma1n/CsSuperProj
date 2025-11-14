using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace App1.Models
{
    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string ImageUrl { get; set; } = string.Empty;
        
        public string Duration { get; set; } = string.Empty;
        
        public string Level { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties - ВАЖНО: должно быть именно Lessons (множественное число)
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();
        
        public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}