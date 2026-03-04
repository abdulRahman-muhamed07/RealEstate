using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Models;

namespace RealEstate.DataAccess
{
    public static class DbInitializer
    {
        public static async Task Seed(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //  -------- Migrates all data  -------- 
            await context.Database.MigrateAsync();

            // --------  Creating Rules  -------- 
            if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));

            //   --------  Creating a default admin user  -------- 
            var adminEmail = "admin@realestate.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Mano",      
                    LastName = "Admin",     
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            //  --------  Creating a default  user  -------- 
            var userEmail = "user@gmail.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var normalUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = "Btabeto",
                    LastName = "bot",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(normalUser, "User@123");
                await userManager.AddToRoleAsync(normalUser, "User");
            }

            //  -------- adding Categories -------- 
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(new List<Category>
                {
                    new Category { Name = "شقق سكنية" },
                    new Category { Name = "فيلات فاخرة" },
                    new Category { Name = "شاليهات" },
                    new Category { Name = "محل تجاري" }


                });
                await context.SaveChangesAsync();
            }
        }
    }
}