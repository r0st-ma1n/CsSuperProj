using App1.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Review> Reviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.CourseId }).IsUnique();
        });
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Явно указываем названия таблиц для избежания конфликтов регистра
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Course>().ToTable("Courses");
        modelBuilder.Entity<Lesson>().ToTable("Lessons");
        modelBuilder.Entity<Enrollment>().ToTable("Enrollments");
        modelBuilder.Entity<UserProgress>().ToTable("UserProgresses");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(255);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(255);
            entity.Property(c => c.Description).IsRequired();
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
            entity.Property(c => c.Duration).HasMaxLength(50);
            entity.Property(c => c.Level).HasMaxLength(50);
            entity.Property(c => c.CreatedAt).IsRequired();
        });

        // Configure Lesson entity
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(255);
            entity.Property(l => l.Content).IsRequired();
            entity.Property(l => l.Duration).HasMaxLength(50);
            entity.Property(l => l.OrderNumber).IsRequired();

            // Relationship with Course
            entity.HasOne(l => l.Course)
                  .WithMany(c => c.Lessons)
                  .HasForeignKey(l => l.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Enrollment entity
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EnrolledAt).IsRequired();
            entity.Property(e => e.Progress).HasPrecision(5, 2);

            // Relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Enrollments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - user can enroll in course only once
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
        });

        // Configure UserProgress entity
        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(up => up.Id);
            entity.Property(up => up.CompletedAt).IsRequired(false);

            // Relationships
            entity.HasOne(up => up.User)
                  .WithMany(u => u.Progresses)
                  .HasForeignKey(up => up.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Lesson)
                  .WithMany(l => l.Progresses)
                  .HasForeignKey(up => up.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Course)
                  .WithMany()
                  .HasForeignKey(up => up.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint - user can complete lesson only once
            entity.HasIndex(up => new { up.UserId, up.LessonId }).IsUnique();
        });
    }


}