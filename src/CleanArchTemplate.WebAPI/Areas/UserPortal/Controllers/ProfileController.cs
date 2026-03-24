using Asp.Versioning;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Profile.Commands;
using CleanArchTemplate.Application.Profile.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchTemplate.WebAPI.Areas.UserPortal.Controllers;

[ApiController]
[Area("UserPortal")]
[Route("api/v{version:apiVersion}/user/profile")]
[ApiVersion("1.0")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get the authenticated user's profile.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(), ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value));
    }

    /// <summary>Update the authenticated user's profile.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Profile updated successfully."));
    }

    /// <summary>Upload a profile image (JPEG, PNG, WEBP — max 5 MB).</summary>
    [HttpPost("image")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadProfileImage(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse.Fail(ErrorCodes.RequiredFieldMissing, "No file was uploaded."));

        var command = new UploadProfileImageCommand
        {
            ImageStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length
        };

        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Profile image uploaded successfully."));
    }

    /// <summary>Schedule the authenticated user's account for deletion (30-day grace period).</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return StatusCode(result.HttpStatusCode, ApiResponse.Fail(result.ErrorCode!, result.ErrorMessage!));

        return Ok(ApiResponse.Ok(result.Value, "Account scheduled for deletion."));
    }
}
