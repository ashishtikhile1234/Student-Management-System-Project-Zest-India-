using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SMS.Application.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Exceptions;
using SMS.Infrastructure.Repositories;

namespace SMS.Application.Services;

/// <summary>
/// Handles user registration and JWT token generation.
/// Passwords are hashed with BCrypt. Tokens are signed with HMAC-SHA256.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository  = userRepository;
        _configuration   = configuration;
        _logger          = logger;
    }

    /// <inheritdoc/>
    public async Task RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registration attempt for username: {Username}", dto.Username);

        if (await _userRepository.UsernameExistsAsync(dto.Username))
        {
            _logger.LogWarning("Registration failed — username already exists: {Username}", dto.Username);
            throw new ConflictException($"Username '{dto.Username}' is already taken.");
        }

        var user = new User
        {
            Username     = dto.Username.Trim(),
            // BCrypt automatically handles salting
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = "Admin"
        };

        await _userRepository.AddAsync(user);
        _logger.LogInformation("User registered successfully: {Username}", dto.Username);
    }

    /// <inheritdoc/>
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Login attempt for username: {Username}", dto.Username);

        var user = await _userRepository.GetByUsernameAsync(dto.Username);

        // Verify user exists and password matches the hash
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username: {Username}", dto.Username);
            throw new SMS.Domain.Exceptions.ValidationException("Invalid username or password.");
        }

        var token    = GenerateJwtToken(user);
        var expiry   = DateTime.UtcNow.AddMinutes(GetExpiryMinutes());

        _logger.LogInformation("Login successful for username: {Username}", dto.Username);

        return new AuthResponseDto
        {
            Token     = token,
            ExpiresAt = expiry,
            Username  = user.Username,
            Role      = user.Role
        };
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey   = jwtSettings["SecretKey"]
                          ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             jwtSettings["Issuer"],
            audience:           jwtSettings["Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(GetExpiryMinutes()),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetExpiryMinutes()
    {
        var raw = _configuration["JwtSettings:ExpiryInMinutes"];
        return int.TryParse(raw, out var mins) ? mins : 60;
    }
}
