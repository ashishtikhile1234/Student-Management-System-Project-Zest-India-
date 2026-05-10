namespace SMS.Domain.Entities;

/// <summary>
/// Represents a student record in the system.
/// </summary>
public class Student
{
    /// <summary>Primary key, auto-incremented.</summary>
    public int Id { get; set; }

    /// <summary>Full name of the student.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Unique email address of the student.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Age of the student (1–120).</summary>
    public int Age { get; set; }

    /// <summary>Course the student is enrolled in.</summary>
    public string Course { get; set; } = string.Empty;

    /// <summary>UTC date/time when the record was created.</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
