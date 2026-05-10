using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SMS.API.Controllers;
using SMS.Application.DTOs;
using SMS.Application.Interfaces;
using SMS.Domain.Exceptions;
using Xunit;

namespace SMS.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="StudentsController"/>.
/// Verifies HTTP status codes and response shapes without hitting the database.
/// </summary>
public class StudentsControllerTests
{
    private readonly Mock<IStudentService> _mockService;
    private readonly Mock<ILogger<StudentsController>> _mockLogger;
    private readonly StudentsController _controller;

    public StudentsControllerTests()
    {
        _mockService = new Mock<IStudentService>();
        _mockLogger  = new Mock<ILogger<StudentsController>>();
        _controller  = new StudentsController(_mockService.Object, _mockLogger.Object);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturn200_WithStudentList()
    {
        var students = new List<StudentDto>
        {
            new() { Id = 1, Name = "Rahul", Email = "r@r.com", Age = 21, Course = "CS" }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(students);

        var result = await _controller.GetAll() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ShouldReturn200_WhenStudentExists()
    {
        var student = new StudentDto { Id = 1, Name = "Rahul", Email = "r@r.com", Age = 21, Course = "CS" };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(student);

        var result = await _controller.GetById(1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldPropagateNotFoundException_WhenStudentMissing()
    {
        _mockService.Setup(s => s.GetByIdAsync(99)).ThrowsAsync(new NotFoundException("Student", 99));
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(99));
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenStudentCreated()
    {
        var dto     = new CreateStudentDto { Name = "Rahul", Email = "r@r.com", Age = 21, Course = "CS" };
        var created = new StudentDto { Id = 1, Name = "Rahul", Email = "r@r.com", Age = 21, Course = "CS" };
        _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldPropagateConflictException_WhenEmailDuplicate()
    {
        var dto = new CreateStudentDto { Name = "X", Email = "dup@dup.com", Age = 20, Course = "IT" };
        _mockService.Setup(s => s.CreateAsync(dto)).ThrowsAsync(new ConflictException("Email exists."));
        await Assert.ThrowsAsync<ConflictException>(() => _controller.Create(dto));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturn200_WhenStudentUpdated()
    {
        var dto     = new UpdateStudentDto { Name = "Rahul U", Email = "r@r.com", Age = 22, Course = "IT" };
        var updated = new StudentDto { Id = 1, Name = "Rahul U", Email = "r@r.com", Age = 22, Course = "IT" };
        _mockService.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(1, dto) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturn200_WhenStudentDeleted()
    {
        _mockService.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);
        var result = await _controller.Delete(1) as OkObjectResult;
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldPropagateNotFoundException_WhenStudentMissing()
    {
        _mockService.Setup(s => s.DeleteAsync(99)).ThrowsAsync(new NotFoundException("Student", 99));
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.Delete(99));
    }
}
