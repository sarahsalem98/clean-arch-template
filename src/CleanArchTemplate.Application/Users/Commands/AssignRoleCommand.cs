using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Users.DTOs;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Exceptions;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Users.Commands;

public record AssignRoleCommand : IRequest<Result<UserAdminDto>>
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<UserAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserAdminDto>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Role", request.RoleId);

        var alreadyHasRole = user.UserRoles.Any(ur => ur.RoleId == request.RoleId);
        if (!alreadyHasRole)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = request.UserId,
                RoleId = request.RoleId
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Reload to get updated data
        await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        return Result<UserAdminDto>.Success(user.Adapt<UserAdminDto>());
    }
}

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required.")
            .MustAsync(async (roleId, ct) =>
                await _context.Roles.AnyAsync(r => r.Id == roleId, ct))
            .WithMessage("The specified role does not exist.");
    }
}
