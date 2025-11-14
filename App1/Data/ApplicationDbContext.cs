using App1.Models;
using Microsoft.EntityFrameworkCore;

namespace App1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<UserProgress> UserProgresses { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Явно указываем названия таблиц
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Course>().ToTable("Courses");
            modelBuilder.Entity<Lesson>().ToTable("Lessons");
            modelBuilder.Entity<Enrollment>().ToTable("Enrollments");
            modelBuilder.Entity<UserProgress>().ToTable("UserProgresses");
            modelBuilder.Entity<Review>().ToTable("Reviews");

            // Конфигурация User
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

            // Конфигурация Course
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

            // Конфигурация Lesson
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Title).IsRequired().HasMaxLength(255);
                entity.Property(l => l.Content).IsRequired();
                entity.Property(l => l.Duration).HasMaxLength(50);
                entity.Property(l => l.OrderNumber).IsRequired();

                // Relationship with Course - ИСПРАВЛЕНО
                entity.HasOne(l => l.Course)
                      .WithMany(c => c.Lessons) // Убедитесь, что в Course есть свойство Lessons
                      .HasForeignKey(l => l.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Enrollment
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

                // Unique constraint
                entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            });

            // Конфигурация UserProgress
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
                      .WithMany() // Course не имеет навигационного свойства к UserProgress
                      .HasForeignKey(up => up.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(up => new { up.UserId, up.LessonId }).IsUnique();
            });

            // Конфигурация Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                // Разрешить multiple reviews per course, но только один на урок
                entity.HasIndex(r => new { r.UserId, r.CourseId, r.LessonId }).IsUnique();

                // Relationships
                entity.HasOne(r => r.User)
                      .WithMany() // User не имеет навигационного свойства к Review
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Course)
                      .WithMany() // Course не имеет навигационного свойства к Review
                      .HasForeignKey(r => r.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Дополнительная связь с Lesson (если нужно)
                entity.HasOne(r => r.Lesson)
                      .WithMany() // Lesson не имеет навигационного свойства к Review
                      .HasForeignKey(r => r.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}