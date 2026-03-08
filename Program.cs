
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.DataAccess;
using RealEstate.Models;
using RealEstate.Services;
using System.Threading.Tasks;

namespace RealEstate
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;        
                options.User.RequireUniqueEmail = true;     
            })
                 .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddScoped<IPropertyService, PropertyService>();

            builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // json circle
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddHttpContextAccessor();
            var app = builder.Build();
            await DbInitializer.Seed(app);
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
