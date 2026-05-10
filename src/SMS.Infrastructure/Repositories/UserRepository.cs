using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Repositories;

/// <summary>
/// Repository for User data access — used by the Auth service.
/// </summary>
public class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Find a user by their username (case-insensitive).</summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
                             .AsNoTracking()
                             .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>Returns true if a user with the given username already exists.</summary>
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users
                             .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>Adds a new user record to the database.</summary>
    public async Task<User> AddAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
