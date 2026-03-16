using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Users.DTOs;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Users.Commands;

public record UpdateUserStatusCommand : IRequest<Result<UserAdminDto>>
{
    public Guid UserId { get; init; }
    public string Status { get; init; } = default!;
}

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Result<UserAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserAdminDto>> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserStatus>(request.Status, true, out var status))
            throw new DomainValidationException("status",
                "Status must be 'active', 'inactive', or 'suspended'.");

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        user.Status = status;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<UserAdminDto>.Success(user.Adapt<UserAdminDto>());
    }
}
