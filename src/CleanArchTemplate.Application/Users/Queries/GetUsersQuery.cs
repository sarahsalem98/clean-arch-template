using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Users.DTOs;
using CleanArchTemplate.Domain.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Users.Queries;

public record GetUsersQuery : IRequest<Result<PaginatedResult<UserAdminDto>>>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string SortBy { get; init; } = "createdAt";
    public string SortOrder { get; init; } = "desc";
    public string? Search { get; init; }
    public string? FilterStatus { get; init; }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PaginatedResult<UserAdminDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<UserAdminDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Min(request.Limit, 100);
        var page = Math.Max(request.Page, 1);

        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.Contains(search) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search));
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(request.FilterStatus) &&
            Enum.TryParse<UserStatus>(request.FilterStatus, true, out var status))
        {
            query = query.Where(u => u.Status == status);
        }

        // Sort
        query = request.SortBy.ToLower() switch
        {
            "email" => request.SortOrder == "asc"
                ? query.OrderBy(u => u.Email)
                : query.OrderByDescending(u => u.Email),
            "firstname" => request.SortOrder == "asc"
                ? query.OrderBy(u => u.FirstName)
                : query.OrderByDescending(u => u.FirstName),
            _ => request.SortOrder == "asc"
                ? query.OrderBy(u => u.CreatedAt)
                : query.OrderByDescending(u => u.CreatedAt)
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var dtos = items.Adapt<List<UserAdminDto>>();

        return Result<PaginatedResult<UserAdminDto>>.Success(new PaginatedResult<UserAdminDto>
        {
            Items = dtos,
            Pagination = PaginationMetadata.Create(page, limit, totalItems)
        });
    }
}
