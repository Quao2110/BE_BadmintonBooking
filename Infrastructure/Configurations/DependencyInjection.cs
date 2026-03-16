using Application.Interfaces.IRepositories;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using Application.Options;
using Application.Services;
using Infrastructure.DbContexts;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BadmintonBooking_PRM393;User ID=sa;Password=123456;Encrypt=False;TrustServerCertificate=True;";
                //throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            services.AddDbContext<BadmintonBooking_PRM393Context>(options =>
                options.UseSqlServer(connectionString));
            services.Configure<BookingOptions>(configuration.GetSection("BookingOptions"));

            // 2. C?u h�nh UnitOfWork & Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // 3. C?u h�nh Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<ICourtService, CourtService>();
            services.AddScoped<ICourtImageService, CourtImageService>();
            services.AddScoped<IShopService, ShopService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            // AI service for auto-replies
            services.AddScoped<IAiService, AiService>();
            services.Configure<AiOptions>(configuration.GetSection("AiOptions"));

            // 4. C?u h�nh AutoMapper (Gom lu�n v�o d�y cho Program.cs d? ch?t)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
