using Asp.Versioning;
using CleanArchTemplate.Application.Auth.Commands;
using CleanArchTemplate.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.WebAPI.Areas.UserPortal.Controllers;

[ApiController]
[Area("UserPortal")]
[Route("api/v{version:apiVersion}/user/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return StatusCode(result.HttpStatusCode, ApiResponse.Ok(result.Value, "Registration successful."));
    }

    /// <summary>Authenticate with email and password.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Login successful."));
    }

    /// <summary>Refresh access token using a valid refresh token.</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value));
    }

    /// <summary>Request a password reset email.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "If the email exists, a reset link has been sent."));
    }

    /// <summary>Reset password using a token received by email.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Password has been reset successfully."));
    }

    /// <summary>Change password for the authenticated user.</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Password changed successfully."));
    }

    /// <summary>Logout and revoke all active tokens.</summary>
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

    /// <summary>Verify email address with a token.</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Email verified successfully."));
    }

    /// <summary>Authenticate or register via social provider (Google, Apple, Facebook).</summary>
    [HttpPost("social-login")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, result.Value?.IsNewUser == true ? "Registration successful." : "Login successful."));
    }
}
