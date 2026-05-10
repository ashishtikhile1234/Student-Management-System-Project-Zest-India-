using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.DTOs;
using SMS.Application.Interfaces;

namespace SMS.API.Controllers;

/// <summary>
/// Manages student records. All endpoints require a valid JWT Bearer token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger         = logger;
    }

    /// <summary>Get all students.</summary>
    /// <response code="200">Returns the list of all students.</response>
    /// <response code="401">Unauthorized — valid JWT required.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GET /api/students called.");
        var students = await _studentService.GetAllAsync();
        var list = students.ToList();
        return Ok(ApiResponse<IEnumerable<StudentDto>>.Ok(list, count: list.Count));
    }

    /// <summary>Get a student by ID.</summary>
    /// <param name="id">The unique identifier of the student.</param>
    /// <response code="200">Returns the student.</response>
    /// <response code="401">Unauthorized — valid JWT required.</response>
    /// <response code="404">Student not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("GET /api/students/{Id} called.", id);
        var student = await _studentService.GetByIdAsync(id);
        return Ok(ApiResponse<StudentDto>.Ok(student));
    }

    /// <summary>Add a new student.</summary>
    /// <param name="dto">Student creation payload.</param>
    /// <response code="201">Student created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — valid JWT required.</response>
    /// <response code="409">Email already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
    {
        _logger.LogInformation("POST /api/students called.");
        var created = await _studentService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<StudentDto>.Ok(created, "Student created successfully."));
    }

    /// <summary>Update an existing student.</summary>
    /// <param name="id">The unique identifier of the student to update.</param>
    /// <param name="dto">Updated student data.</param>
    /// <response code="200">Student updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — valid JWT required.</response>
    /// <response code="404">Student not found.</response>
    /// <response code="409">Email already registered to another student.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
    {
        _logger.LogInformation("PUT /api/students/{Id} called.", id);
        var updated = await _studentService.UpdateAsync(id, dto);
        return Ok(ApiResponse<StudentDto>.Ok(updated, "Student updated successfully."));
    }

    /// <summary>Delete a student by ID.</summary>
    /// <param name="id">The unique identifier of the student to delete.</param>
    /// <response code="200">Student deleted successfully.</response>
    /// <response code="401">Unauthorized — valid JWT required.</response>
    /// <response code="404">Student not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/students/{Id} called.", id);
        await _studentService.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Student deleted successfully."));
    }
}
