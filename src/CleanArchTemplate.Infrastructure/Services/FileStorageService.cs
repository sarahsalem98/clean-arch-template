using CleanArchTemplate.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private const string FakeCdnBase = "https://cdn.example.com/uploads";
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public Task<(string ImageUrl, string ThumbnailUrl)> UploadProfileImageAsync(
        Stream imageStream, string fileName, string contentType,
        CancellationToken cancellationToken = default)
    {
        var uniqueName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var imageUrl = $"{FakeCdnBase}/profiles/{uniqueName}";
        var thumbnailUrl = $"{FakeCdnBase}/profiles/thumbs/{uniqueName}";

        _logger.LogInformation("[STORAGE STUB] Uploaded profile image: {Url}", imageUrl);

        return Task.FromResult((imageUrl, thumbnailUrl));
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[STORAGE STUB] Deleted file: {Url}", fileUrl);
        return Task.CompletedTask;
    }
}
