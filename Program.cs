using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.DataAccess;
using RealEstate.Models;
using RealEstate.Services;

namespace RealEstate
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------ Database Connection ------
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ------ Identity Configuration ------
            // Note: Added once to avoid "Scheme already exists" error
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ------ Cookie Configuration for API ------
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                // Return 401 instead of redirecting to login page
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

            // ------ Dependency Injection (Services) ------
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IPropertyService, PropertyService>();
            builder.Services.AddHttpContextAccessor();

            // ------ Controllers & JSON Formatting ------
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Prevent circular references in EF Core
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // ------ OpenAPI / Swagger ------
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // ------ Database Seeding ------
            await DbInitializer.Seed(app);

            // ------ HTTP Request Pipeline ------
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // ------ Static Files (Images) ------
            app.UseStaticFiles();

            app.UseRouting();

            // ------ Auth Middleware (Order Matters!) ------
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}