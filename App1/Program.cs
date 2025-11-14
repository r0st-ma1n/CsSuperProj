using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using App1.Data; // Добавьте эту директиву
using App1.Services; // Добавьте эту директиву
using App1.Models; // Добавьте эту директиву

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация сервисов
builder.Services.AddScoped<ReviewService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// HTML маршруты
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapGet("/login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/login.html");
});

app.MapGet("/register", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/register.html");
});

app.MapGet("/dashboard", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/dashboard.html");
});

app.MapGet("/courses", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/courses.html");
});

app.MapGet("/courses/python", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/course-python.html");
});

app.MapGet("/courses/ml", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/course-ml.html");
});

app.MapGet("/courses/asyncio", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/course-asyncio.html");
});

app.MapGet("/about", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/about.html");
});

// Уроки курса Python
app.MapGet("/courses/python/lesson/1", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson1.html");
});

app.MapGet("/courses/python/lesson/2", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson2.html");
});

app.MapGet("/courses/python/lesson/3", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson3.html");
});

app.MapGet("/courses/python/lesson/4", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson4.html");
});

app.MapGet("/courses/python/lesson/5", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson5.html");
});

app.MapGet("/courses/python/lesson/6", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson6.html");
});

app.MapGet("/courses/python/lesson/7", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson7.html");
});

app.MapGet("/courses/python/lesson/8", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson8.html");
});

app.MapGet("/courses/python/lesson/9", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson9.html");
});

app.MapGet("/courses/python/lesson/10", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/lesson10.html");
});

app.MapGet("/courses/python/exam", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/lessons/python/exam.html");
});

// Старые маршруты для обратной совместимости
app.MapGet("/session/new", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/login.html");
});

app.MapGet("/users/new", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/register.html");
});

// API Controllers
app.MapControllers();

// Тестовый endpoint для проверки базы данных
app.MapGet("/test-db-connection", async (ApplicationDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        return Results.Ok(new
        {
            success = true,
            message = "Database connected successfully!",
            canConnect
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

// Создание тестовых данных при запуске
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Создаем тестовые курсы если их нет
    if (!context.Courses.Any())
    {
        var courses = new List<Course>
        {
            new Course
            {
                Title = "Python Programming",
                Description = "Learn Python from scratch to advanced level",
                ImageUrl = "/images/python.jpg",
                Duration = "10 hours",
                Level = "Beginner",
                CreatedAt = DateTime.UtcNow,
                Lessons = new List<Lesson>
                {
                    new Lesson {
                        Title = "Introduction to Python",
                        Content = "Python basics...",
                        OrderNumber = 1,
                        Duration = "1 hour",
                        CourseId = 1
                    },
                    new Lesson {
                        Title = "Data Types and Operations",
                        Content = "Strings, numbers, lists...",
                        OrderNumber = 2,
                        Duration = "1.5 hours",
                        CourseId = 1
                    },
                    new Lesson {
                        Title = "Conditional Statements",
                        Content = "If-else statements...",
                        OrderNumber = 3,
                        Duration = "1 hour",
                        CourseId = 1
                    },
                    new Lesson {
                        Title = "Loops",
                        Content = "For and while loops...",
                        OrderNumber = 4,
                        Duration = "1.5 hours",
                        CourseId = 1
                    },
                    new Lesson {
                        Title = "Functions and Modules",
                        Content = "Functions and modules...",
                        OrderNumber = 5,
                        Duration = "2 hours",
                        CourseId = 1
                    }
                }
            }
        };

        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Тестовые курсы и уроки созданы!");
    }
}

app.Run();