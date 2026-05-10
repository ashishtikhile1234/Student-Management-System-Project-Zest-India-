namespace SMS.Domain.Exceptions;

/// <summary>
/// Thrown when a requested resource is not found (maps to HTTP 404).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string resourceName, int id)
        : base($"{resourceName} with ID {id} was not found.") { }
}

/// <summary>
/// Thrown when a conflict exists (e.g., duplicate email) — maps to HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// Thrown when input validation fails — maps to HTTP 400.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
