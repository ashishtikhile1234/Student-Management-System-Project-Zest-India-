using Microsoft.AspNetCore.Mvc;
using SMS.Application.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers;

/// <summary>
/// Handles user registration and login.
/// These endpoints are publicly accessible (no JWT required).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    /// <summary>Register a new user account.</summary>
    /// <remarks>
    /// Password requirements: min 8 chars, at least 1 uppercase, 1 digit, 1 special character.
    /// </remarks>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Validation error in request body.</response>
    /// <response code="409">Username already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        _logger.LogInformation("Register endpoint called for: {Username}", dto.Username);
        await _authService.RegisterAsync(dto);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<string>.Ok("User registered successfully."));
    }

    /// <summary>Authenticate and receive a JWT Bearer token.</summary>
    /// <remarks>
    /// Use the returned token as: `Authorization: Bearer {token}` on secured endpoints.
    /// </remarks>
    /// <response code="200">Login successful, returns JWT token.</response>
    /// <response code="400">Validation error in request body.</response>
    /// <response code="401">Invalid username or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login endpoint called for: {Username}", dto.Username);
        var result = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }
}
