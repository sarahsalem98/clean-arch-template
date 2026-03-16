using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Users.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Users.Queries;

public record GetAuditLogsQuery : IRequest<Result<PaginatedResult<AuditLogDto>>>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public Guid? UserId { get; init; }
    public string? Action { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedResult<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Min(request.Limit, 100);
        var page = Math.Max(request.Page, 1);

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action.Contains(request.Action.ToUpper()));

        if (request.From.HasValue)
            query = query.Where(a => a.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.CreatedAt <= request.To.Value);

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Result<PaginatedResult<AuditLogDto>>.Success(new PaginatedResult<AuditLogDto>
        {
            Items = items.Adapt<List<AuditLogDto>>(),
            Pagination = PaginationMetadata.Create(page, limit, totalItems)
        });
    }
}
