namespace CleanArchTemplate.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<(string ImageUrl, string ThumbnailUrl)> UploadProfileImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
