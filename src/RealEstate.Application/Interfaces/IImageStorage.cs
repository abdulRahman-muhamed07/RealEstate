using RealEstate.Application.Common;

namespace RealEstate.Application.Interfaces;

public interface IImageStorage
{
    Task<IReadOnlyList<string>> SaveAsync(IEnumerable<UploadedFile> files, CancellationToken ct);
    Task DeleteAsync(IEnumerable<string> urls, CancellationToken ct);
}
