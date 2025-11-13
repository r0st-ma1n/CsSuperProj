public class UserProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public Course Course { get; set; } = null!;
}