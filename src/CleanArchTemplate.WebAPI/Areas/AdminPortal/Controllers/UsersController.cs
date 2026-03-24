using Asp.Versioning;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Users.Commands;
using CleanArchTemplate.Application.Users.Queries;
using CleanArchTemplate.WebAPI.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.WebAPI.Areas.AdminPortal.Controllers;

[ApiController]
[Area("AdminPortal")]
[Route("api/v{version:apiVersion}/admin/users")]
[ApiVersion("1.0")]
[RequirePortal("admin-portal")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get paginated list of users with optional search and filter.</summary>
    [HttpGet]
    [RequirePermission("users.view")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value));
    }

    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{userId:guid}")]
    [RequirePermission("users.view")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery ( userId ), ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value));
    }

    /// <summary>Update a user's status (active / inactive / suspended).</summary>
    [HttpPatch("{userId:guid}/status")]
    [RequirePermission("users.status.update")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(
        Guid userId, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateUserStatusCommand { UserId = userId, Status = request.Status }, ct);

        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "User status updated."));
    }

    /// <summary>Assign a role to a user.</summary>
    [HttpPost("{userId:guid}/roles")]
    [RequirePermission("users.role.assign")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        Guid userId, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AssignRoleCommand { UserId = userId, RoleId = request.RoleId }, ct);

        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Role assigned successfully."));
    }

    /// <summary>Get audit logs, optionally filtered by user.</summary>
    [HttpGet("audit-logs")]
    [RequirePermission("audit-logs.view")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsQuery query, CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value));
    }
}

public record UpdateStatusRequest(string Status);
public record AssignRoleRequest(Guid RoleId);
