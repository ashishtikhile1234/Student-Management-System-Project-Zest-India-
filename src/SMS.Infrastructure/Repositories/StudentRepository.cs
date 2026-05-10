using Microsoft.EntityFrameworkCore;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of <see cref="IStudentRepository"/> using EF Core.
/// All database operations are async and use the injected <see cref="AppDbContext"/>.
/// </summary>
public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
                             .OrderByDescending(s => s.CreatedDate)
                             .AsNoTracking()
                             .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
                             .AsNoTracking()
                             .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _context.Students.Where(s => s.Email.ToLower() == email.ToLower());

        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<Student> AddAsync(Student student)
    {
        student.CreatedDate = DateTime.UtcNow;
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    /// <inheritdoc/>
    public async Task<Student> UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }
}
