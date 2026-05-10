using SMS.Domain.Entities;

namespace SMS.Application.Interfaces;

/// <summary>
/// Repository contract for Student data access operations.
/// The Infrastructure layer implements this, keeping the Application layer DB-agnostic.
/// </summary>
public interface IStudentRepository
{
    /// <summary>Retrieves all students from the database.</summary>
    Task<IEnumerable<Student>> GetAllAsync();

    /// <summary>Retrieves a student by their unique ID. Returns null if not found.</summary>
    Task<Student?> GetByIdAsync(int id);

    /// <summary>Checks whether a student with the given email exists (excludes a given ID for update scenarios).</summary>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    /// <summary>Adds a new student to the database.</summary>
    Task<Student> AddAsync(Student student);

    /// <summary>Updates an existing student in the database.</summary>
    Task<Student> UpdateAsync(Student student);

    /// <summary>Deletes a student from the database.</summary>
    Task DeleteAsync(Student student);
}
