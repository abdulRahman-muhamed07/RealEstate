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
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ------ CORS ------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

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
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
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

           builder.Services.AddControllers()
               .AddJsonOptions(options =>
               {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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


            // ------ Static Files (Images) ------
            app.UseStaticFiles();

            app.UseRouting();

            // ------ CORS ------
            app.UseCors("AllowFrontend");

            // ------ Auth Middleware (Order Matters!) ------
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}