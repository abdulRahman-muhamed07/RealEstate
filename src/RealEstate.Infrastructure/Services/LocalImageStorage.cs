using RealEstate.Application.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Services;

public sealed class LocalImageStorage : IImageStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const int MaxFiles = 8;
    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly string _rootPath;

    public LocalImageStorage(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");

        _rootPath = Path.Combine(webRoot, "uploads", "properties");
    }

    public async Task<IReadOnlyList<string>> SaveAsync(
        IEnumerable<UploadedFile> files,
        CancellationToken ct)
    {
        var items = files?.Take(MaxFiles).ToArray() ?? Array.Empty<UploadedFile>();
        Directory.CreateDirectory(_rootPath);

        var savedUrls = new List<string>(items.Length);

        try
        {
            foreach (var file in items)
            {
                Validate(file);

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var path = Path.Combine(_rootPath, fileName);

                await using var input = file.OpenReadStream();
                await using var output = new FileStream(
                    path,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 64 * 1024,
                    useAsync: true);

                await input.CopyToAsync(output, ct);
                savedUrls.Add($"/uploads/properties/{fileName}");
            }

            return savedUrls;
        }
        catch
        {
            await DeleteAsync(savedUrls, CancellationToken.None);
            throw;
        }
    }

    public Task DeleteAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        foreach (var url in urls ?? Array.Empty<string>())
        {
            ct.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(url);
            if (string.IsNullOrWhiteSpace(fileName))
                continue;

            var path = Path.Combine(_rootPath, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private static void Validate(UploadedFile file)
    {
        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Only JPG, JPEG, PNG and WEBP images are allowed.");

        if (file.Length <= 0 || file.Length > MaxFileSize)
            throw new InvalidOperationException("Each image must be between 1 byte and 5 MB.");
    }
}
