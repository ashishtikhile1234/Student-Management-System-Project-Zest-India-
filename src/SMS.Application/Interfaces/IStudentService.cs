using SMS.Application.DTOs;

namespace SMS.Application.Interfaces;

/// <summary>
/// Service contract for Student business operations.
/// Controllers depend only on this interface, not on concrete implementations.
/// </summary>
public interface IStudentService
{
    /// <summary>Returns all students as DTOs.</summary>
    Task<IEnumerable<StudentDto>> GetAllAsync();

    /// <summary>Returns a single student DTO by ID. Throws NotFoundException if absent.</summary>
    Task<StudentDto> GetByIdAsync(int id);

    /// <summary>Creates a new student. Throws ConflictException on duplicate email.</summary>
    Task<StudentDto> CreateAsync(CreateStudentDto dto);

    /// <summary>Updates an existing student. Throws NotFoundException or ConflictException as appropriate.</summary>
    Task<StudentDto> UpdateAsync(int id, UpdateStudentDto dto);

    /// <summary>Deletes a student by ID. Throws NotFoundException if absent.</summary>
    Task DeleteAsync(int id);
}
