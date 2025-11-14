namespace App1.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public string Duration { get; set; } = string.Empty;
        
        // Navigation properties
        public Course Course { get; set; } = null!;
        public List<UserProgress> Progresses { get; set; } = new();
    }
}