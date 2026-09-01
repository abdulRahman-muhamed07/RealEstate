using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstate.Infrastructure;
using RealEstate.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
    o.TokenValidationParameters = new TokenValidationParameters { ValidateIssuerSigningKey=true, IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), ValidateIssuer=true, ValidIssuer=builder.Configuration["Jwt:Issuer"], ValidateAudience=true, ValidAudience=builder.Configuration["Jwt:Audience"], ValidateLifetime=true, ClockSkew=TimeSpan.FromSeconds(30) };
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => { o.SwaggerDoc("v1", new OpenApiInfo { Title="Smart Real Estate API", Version="v1" }); o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name="Authorization", Type=SecuritySchemeType.Http, Scheme="bearer", BearerFormat="JWT", In=ParameterLocation.Header }); o.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference=new OpenApiReference { Type=ReferenceType.SecurityScheme, Id="Bearer" } }, Array.Empty<string>() } }); });
var app = builder.Build();
using (var scope = app.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); await db.Database.EnsureCreatedAsync(); await SeedData.SeedAsync(scope.ServiceProvider); }
app.UseSwagger(); app.UseSwaggerUI(); app.UseStaticFiles(); app.UseCors("Frontend"); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); app.Run();

static class SeedData
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var db=sp.GetRequiredService<AppDbContext>(); var hasher=sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Builder.WebApplicationOptions>>();
        if (await db.Users.AnyAsync()) return;
        var passwordHasher=new Microsoft.AspNetCore.Identity.PasswordHasher<RealEstate.Domain.Entities.User>();
        var admin=new RealEstate.Domain.Entities.User { FirstName="System", LastName="Admin", Email="admin@smartrealestate.local", Role=RealEstate.Domain.Entities.UserRole.Admin }; admin.PasswordHash=passwordHasher.HashPassword(admin,"Password123!");
        var vendor=new RealEstate.Domain.Entities.User { FirstName="Demo", LastName="Vendor", Email="vendor@smartrealestate.local", Role=RealEstate.Domain.Entities.UserRole.Vendor }; vendor.PasswordHash=passwordHasher.HashPassword(vendor,"Password123!");
        db.Users.AddRange(admin,vendor);
        db.Properties.Add(new RealEstate.Domain.Entities.Property { Title="Modern Cairo Apartment", Description="Demo approved property", Price=2500000, Area=180, Bedrooms=3, Bathrooms=2, Type="apartment", ListingType=RealEstate.Domain.Entities.ListingType.Sale, Location="New Cairo, Egypt", CategoryId=1, CityId=4, OwnerId=vendor.Id, IsApproved=true });
        await db.SaveChangesAsync();
    }
}
