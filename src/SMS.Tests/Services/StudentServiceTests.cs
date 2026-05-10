using Microsoft.Extensions.Logging;
using Moq;
using SMS.Application.DTOs;
using SMS.Application.Interfaces;
using SMS.Application.Services;
using SMS.Domain.Entities;
using SMS.Domain.Exceptions;
using Xunit;

namespace SMS.Tests.Services;

/// <summary>
/// Unit tests for <see cref="StudentService"/>.
/// Uses Moq to mock the repository and logger — no database required.
/// </summary>
public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _mockRepo;
    private readonly Mock<ILogger<StudentService>> _mockLogger;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _mockRepo   = new Mock<IStudentRepository>();
        _mockLogger = new Mock<ILogger<StudentService>>();
        _service    = new StudentService(_mockRepo.Object, _mockLogger.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllStudents()
    {
        // Arrange
        var students = new List<Student>
        {
            new() { Id = 1, Name = "Rahul", Email = "rahul@test.com", Age = 21, Course = "CS" },
            new() { Id = 2, Name = "Priya", Email = "priya@test.com", Age = 22, Course = "IT" }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(students);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoStudents()
    {
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Student>());
        var result = await _service.GetAllAsync();
        Assert.Empty(result);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnStudent_WhenExists()
    {
        // Arrange
        var student = new Student { Id = 1, Name = "Rahul", Email = "rahul@test.com", Age = 21, Course = "CS" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Rahul", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenStudentDoesNotExist()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(99));
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldCreateStudent_WhenEmailIsUnique()
    {
        // Arrange
        var dto = new CreateStudentDto { Name = "Rahul", Email = "rahul@test.com", Age = 21, Course = "CS" };
        _mockRepo.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Student>())).ReturnsAsync((Student s) =>
        {
            s.Id = 1;
            return s;
        });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("rahul@test.com", result.Email);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var dto = new CreateStudentDto { Name = "Rahul", Email = "rahul@test.com", Age = 21, Course = "CS" };
        _mockRepo.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(dto));
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStudent_WhenValidData()
    {
        // Arrange
        var existing = new Student { Id = 1, Name = "Rahul", Email = "rahul@test.com", Age = 21, Course = "CS" };
        var dto = new UpdateStudentDto { Name = "Rahul Updated", Email = "rahul@test.com", Age = 22, Course = "IT" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.EmailExistsAsync(dto.Email, 1)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.Equal("Rahul Updated", result.Name);
        Assert.Equal(22, result.Age);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenStudentDoesNotExist()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);
        var dto = new UpdateStudentDto { Name = "X", Email = "x@x.com", Age = 20, Course = "CS" };
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(99, dto));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldDelete_WhenStudentExists()
    {
        var student = new Student { Id = 1, Name = "Rahul", Email = "r@r.com", Age = 21, Course = "CS" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);
        _mockRepo.Setup(r => r.DeleteAsync(student)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(1);

        _mockRepo.Verify(r => r.DeleteAsync(student), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenStudentDoesNotExist()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(99));
    }
}
