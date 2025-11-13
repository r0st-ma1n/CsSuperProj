public class Enrollment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public decimal Progress { get; set; } // 0-100%
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}