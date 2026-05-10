namespace SMS.Domain.Entities;

/// <summary>
/// Represents a system user who can authenticate via JWT.
/// </summary>
public class User
{
    /// <summary>Primary key, auto-incremented.</summary>
    public int Id { get; set; }

    /// <summary>Unique login username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>BCrypt-hashed password — never stored as plain text.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>User role (e.g., "Admin"). Used for role-based authorization.</summary>
    public string Role { get; set; } = "Admin";

    /// <summary>UTC date/time when the user account was created.</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
