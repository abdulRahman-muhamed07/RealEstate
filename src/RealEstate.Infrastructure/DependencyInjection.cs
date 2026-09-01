using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;
using RealEstate.Infrastructure.Services;

namespace RealEstate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(5)));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IImageStorage, LocalImageStorage>();
        services.AddHttpContextAccessor();

        return services;
    }
}
