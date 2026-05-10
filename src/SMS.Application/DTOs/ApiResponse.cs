namespace SMS.Application.DTOs;

/// <summary>
/// Standard API response wrapper used for all endpoints.
/// Ensures consistent response shape across the entire API.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>Indicates whether the request was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Human-readable message describing the outcome.</summary>
    public string? Message { get; set; }

    /// <summary>The response payload.</summary>
    public T? Data { get; set; }

    /// <summary>Validation error details, keyed by field name.</summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>Total count of items (used for list responses).</summary>
    public int? Count { get; set; }

    // ── Factory helpers ────────────────────────────────────────────────

    public static ApiResponse<T> Ok(T data, string? message = null, int? count = null) =>
        new() { Success = true, Data = data, Message = message, Count = count };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };

    public static ApiResponse<T> ValidationFail(Dictionary<string, string[]> errors) =>
        new() { Success = false, Errors = errors };
}
