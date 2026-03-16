using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Profile.DTOs;
using CleanArchTemplate.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Profile.Commands;

public record UploadProfileImageCommand : IRequest<Result<ProfileImageResponseDto>>
{
    public Stream ImageStream { get; init; } = default!;
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long FileSizeBytes { get; init; }
}

public class UploadProfileImageCommandHandler : IRequestHandler<UploadProfileImageCommand, Result<ProfileImageResponseDto>>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public UploadProfileImageCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProfileImageResponseDto>> Handle(UploadProfileImageCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new DomainValidationException("image", "File type not allowed. Allowed types: JPG, PNG, WEBP.");

        if (request.FileSizeBytes > MaxFileSizeBytes)
            throw new DomainValidationException("image", "File size exceeds the 5MB limit.");

        var userId = _currentUserService.UserId
            ?? throw UnauthorizedException.TokenInvalid();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var (imageUrl, thumbnailUrl) = await _fileStorageService.UploadProfileImageAsync(
            request.ImageStream, request.FileName, request.ContentType, cancellationToken);

        user.ProfileImage = imageUrl;
        user.ThumbnailImage = thumbnailUrl;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProfileImageResponseDto>.Success(new ProfileImageResponseDto
        {
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl
        });
    }
}
