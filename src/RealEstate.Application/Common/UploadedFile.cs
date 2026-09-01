namespace RealEstate.Application.Common;

public sealed record UploadedFile(
    string FileName,
    string ContentType,
    long Length,
    Func<Stream> OpenReadStream);
