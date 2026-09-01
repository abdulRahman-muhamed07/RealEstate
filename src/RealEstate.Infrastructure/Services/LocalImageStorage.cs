using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RealEstate.Application.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Services;

public sealed class LocalImageStorage : IImageStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalImageStorage(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<string>> SaveAsync(IEnumerable<UploadedFile> files, CancellationToken ct)
    {
        var items = files?.Take(8).ToArray() ?? Array.Empty<UploadedFile>();
        var root = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "properties");
        Directory.CreateDirectory(root);
        var saved = new List<string>();
        try
        {
            foreach (var file in items)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!AllowedExtensions.Contains(extension)) throw new InvalidOperationException("Only JPG, JPEG, PNG and WEBP images are allowed.");
                if (file.Length <= 0 || file.Length > MaxFileSize) throw new InvalidOperationException("Each image must be between 1 byte and 5 MB.");
                var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var path = Path.Combine(root, fileName);
                await using var input = file.OpenReadStream();
                await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await input.CopyToAsync(output, ct);
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request is null ? string.Empty : $"{request.Scheme}://{request.Host}";
                saved.Add($"{baseUrl}/uploads/properties/{fileName}");
            }
            return saved;
        }
        catch
        {
            await DeleteAsync(saved, ct);
            throw;
        }
    }

    public Task DeleteAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        var root = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "properties");
        foreach (var url in urls ?? Array.Empty<string>())
        {
            var fileName = Path.GetFileName(url);
            if (string.IsNullOrWhiteSpace(fileName)) continue;
            var path = Path.Combine(root, fileName);
            if (File.Exists(path)) File.Delete(path);
        }
        return Task.CompletedTask;
    }
}
