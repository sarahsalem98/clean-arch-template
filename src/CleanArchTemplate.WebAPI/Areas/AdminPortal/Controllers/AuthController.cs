using Asp.Versioning;
using CleanArchTemplate.Application.Auth.Commands;
using CleanArchTemplate.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.WebAPI.Areas.AdminPortal.Controllers;

[ApiController]
[Area("AdminPortal")]
[Route("api/v{version:apiVersion}/admin/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Admin login — requires Admin or SuperAdmin role.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] AdminLoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Admin login successful."));
    }

    /// <summary>Refresh admin access token.</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] AdminRefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value));
    }

    /// <summary>Logout and revoke all active admin tokens.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Logged out successfully."));
    }
}
