using Infrastructure.Configurations;
using WebAPI.Extensions;
using Infrastructure.DbContexts;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerSecurity();

var app = builder.Build();
// Ensure a system bot user exists for AI auto-replies
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<BadmintonBooking_PRM393Context>();
        if (!db.Users.Any(u => u.Role == "System"))
        {
            var bot = new User
            {
                Email = "system@badminton.com",
                FullName = "System Bot",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = "System",
                IsActive = true,
                IsTwoFactorEnabled = false,
                AvatarUrl = "https://img.com/system-bot.png",
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(bot);
            db.SaveChanges();
            Console.WriteLine("System bot user created: system@badminton.com");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating system bot user: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Badminton Booking API v1");
});

app.UseStaticFiles(); // serve wwwroot (avatars, ...)
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Xác thực & Phân quyền
#if DEBUG == false
    app.UseAuthentication();
    app.UseAuthorization();
#else
// Khi Debug: Có thể log ra để biết đang tắt bảo mật
Console.WriteLine("⚠️ Debug Mode: Authentication & Authorization are disabled.");
#endif

app.MapControllers();

app.Run();