using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RealEstate.Application.Contracts;

namespace RealEstate.Infrastructure.Services;

public sealed class LocalImageStorage(IWebHostEnvironment env, IHttpContextAccessor accessor) : IImageStorage
{
    private static readonly string[] Allowed = [".jpg", ".jpeg", ".png", ".webp"];
    public async Task<IReadOnlyList<string>> SaveAsync(IEnumerable<IFormFile> files, CancellationToken ct)
    {
        var list = files?.Where(x => x.Length > 0).Take(8).ToList() ?? [];
        var root = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads", "properties"); Directory.CreateDirectory(root);
        var urls = new List<string>();
        foreach (var file in list)
        {
            if (!Allowed.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())) throw new InvalidOperationException("Only JPG, JPEG, PNG and WEBP images are allowed.");
            if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Each image must be 5 MB or less.");
            var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}"; var path = Path.Combine(root, name);
            await using var stream = File.Create(path); await file.CopyToAsync(stream, ct);
            var baseUrl = $"{accessor.HttpContext?.Request.Scheme}://{accessor.HttpContext?.Request.Host}";
            urls.Add($"{baseUrl}/uploads/properties/{name}");
        }
        return urls;
    }
    public Task DeleteAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        foreach (var url in urls ?? []) { var name = Path.GetFileName(url); if (string.IsNullOrWhiteSpace(name)) continue; var path = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath,"wwwroot"),"uploads","properties",name); if (File.Exists(path)) File.Delete(path); }
        return Task.CompletedTask;
    }
}
