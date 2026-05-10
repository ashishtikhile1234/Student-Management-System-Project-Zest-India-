using SMS.Application.DTOs;

namespace SMS.Application.Interfaces;

/// <summary>
/// Service contract for authentication operations (registration and login).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user. Throws ConflictException if the username already exists.
    /// Hashes the password with BCrypt before storing.
    /// </summary>
    Task RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Validates credentials and returns a JWT token response.
    /// Throws UnauthorizedException if credentials are invalid.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
