using Microsoft.Extensions.Logging;
using SMS.Application.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Exceptions;

namespace SMS.Application.Services;

/// <summary>
/// Business logic for student CRUD operations.
/// Handles validation, mapping between entities and DTOs, and delegates data access to the repository.
/// </summary>
public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository repository, ILogger<StudentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StudentDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all students.");
        var students = await _repository.GetAllAsync();
        return students.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<StudentDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching student with ID {StudentId}.", id);
        var student = await _repository.GetByIdAsync(id)
                      ?? throw new NotFoundException("Student", id);
        return MapToDto(student);
    }

    /// <inheritdoc/>
    public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
    {
        _logger.LogInformation("Creating student with email {Email}.", dto.Email);

        // Check for duplicate email
        if (await _repository.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Duplicate email attempt: {Email}", dto.Email);
            throw new ConflictException($"A student with email '{dto.Email}' already exists.");
        }

        var student = new Student
        {
            Name   = dto.Name.Trim(),
            Email  = dto.Email.Trim().ToLower(),
            Age    = dto.Age,
            Course = dto.Course.Trim()
        };

        var created = await _repository.AddAsync(student);
        _logger.LogInformation("Student created successfully with ID {StudentId}.", created.Id);
        return MapToDto(created);
    }

    /// <inheritdoc/>
    public async Task<StudentDto> UpdateAsync(int id, UpdateStudentDto dto)
    {
        _logger.LogInformation("Updating student with ID {StudentId}.", id);

        var student = await _repository.GetByIdAsync(id)
                      ?? throw new NotFoundException("Student", id);

        // Check duplicate email — exclude current student's ID
        if (await _repository.EmailExistsAsync(dto.Email, excludeId: id))
        {
            _logger.LogWarning("Email conflict on update: {Email}", dto.Email);
            throw new ConflictException($"Email '{dto.Email}' is already registered to another student.");
        }

        student.Name   = dto.Name.Trim();
        student.Email  = dto.Email.Trim().ToLower();
        student.Age    = dto.Age;
        student.Course = dto.Course.Trim();

        var updated = await _repository.UpdateAsync(student);
        _logger.LogInformation("Student ID {StudentId} updated successfully.", id);
        return MapToDto(updated);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting student with ID {StudentId}.", id);

        var student = await _repository.GetByIdAsync(id)
                      ?? throw new NotFoundException("Student", id);

        await _repository.DeleteAsync(student);
        _logger.LogInformation("Student ID {StudentId} deleted successfully.", id);
    }

    // ── Private mapper ────────────────────────────────────────────────────

    private static StudentDto MapToDto(Student s) => new()
    {
        Id          = s.Id,
        Name        = s.Name,
        Email       = s.Email,
        Age         = s.Age,
        Course      = s.Course,
        CreatedDate = s.CreatedDate
    };
}
