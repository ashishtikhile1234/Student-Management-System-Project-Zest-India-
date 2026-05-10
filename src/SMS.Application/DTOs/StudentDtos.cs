using System.ComponentModel.DataAnnotations;

namespace SMS.Application.DTOs;

/// <summary>Read-only DTO returned to clients for student data.</summary>
public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Course { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

/// <summary>DTO for creating a new student. Includes validation annotations.</summary>
public class CreateStudentDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(150, ErrorMessage = "Email must not exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age is required.")]
    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Course must be between 2 and 100 characters.")]
    public string Course { get; set; } = string.Empty;
}

/// <summary>DTO for updating an existing student. All fields required.</summary>
public class UpdateStudentDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(150, ErrorMessage = "Email must not exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age is required.")]
    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Course must be between 2 and 100 characters.")]
    public string Course { get; set; } = string.Empty;
}
