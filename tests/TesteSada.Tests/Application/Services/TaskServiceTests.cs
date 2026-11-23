using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TesteSada.Application.DTOs;
using TesteSada.Application.Exceptions;
using TesteSada.Application.Interfaces;
using TesteSada.Application.Services;
using TesteSada.Application.Validators;
using TesteSada.Domain.Entities;
using TesteSada.Domain.Enums;
using TesteSada.Tests.Helpers;

namespace TesteSada.Tests.Application.Services;

/// <summary>
/// Testes unitários da camada de negócio. O repositório é substituído por um mock (Moq),
/// então nenhum banco de dados é usado: testamos apenas as regras do TaskService.
/// </summary>
public class TaskServiceTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ITaskRepository> _repository = new();
    private readonly FakeTimeProvider _timeProvider = new(Now);
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(
            _repository.Object,
            new CreateTaskRequestValidator(_timeProvider),
            new UpdateTaskRequestValidator(),
            new TaskFilterValidator(),
            _timeProvider,
            NullLogger<TaskService>.Instance);
    }

    private static TaskItem NewTask(string title = "Tarefa", TaskItemStatus status = TaskItemStatus.Pendente) =>
        TaskItem.Create(title, null, null, status, Now.UtcDateTime);

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldPersistAndReturnTaskWithId()
    {
        var request = new CreateTaskRequest
        {
            Title = "Estudar",
            Description = "EF Core",
            DueDate = new DateOnly(2025, 1, 20),
            Status = TaskItemStatus.EmProgresso
        };

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Estudar", result.Title);
        Assert.Equal("EF Core", result.Description);
        Assert.Equal(request.DueDate, result.DueDate);
        Assert.Equal(TaskItemStatus.EmProgresso, result.Status);
        Assert.Equal(Now.UtcDateTime, result.CreatedAt);
        _repository.Verify(r => r.AddAsync(
            It.Is<TaskItem>(t => t.Id == result.Id && t.Title == "Estudar"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ShouldThrowValidationAndNotPersist()
    {
        var request = new CreateTaskRequest { Title = "" };

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request));

        _repository.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DueDateInPast_ShouldThrowValidation()
    {
        var request = new CreateTaskRequest { Title = "Tarefa", DueDate = new DateOnly(2025, 1, 9) };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request));

        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(CreateTaskRequest.DueDate));
    }

    // ---------- Get ----------

    [Fact]
    public async Task GetByIdAsync_Existing_ShouldReturnTask()
    {
        var task = NewTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _service.GetByIdAsync(task.Id);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ShouldThrowNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_ShouldPassFilterToRepositoryAndMapResults()
    {
        var filter = new TaskFilter { Status = TaskItemStatus.Pendente };
        var tasks = new List<TaskItem> { NewTask("A"), NewTask("B") };
        _repository.Setup(r => r.GetAllAsync(filter, It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        var result = await _service.GetAllAsync(filter);

        Assert.Equal(new[] { "A", "B" }, result.Select(t => t.Title));
        _repository.Verify(r => r.GetAllAsync(filter, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_InvalidDateRange_ShouldThrowValidation()
    {
        var filter = new TaskFilter
        {
            DueDateFrom = new DateOnly(2025, 2, 1),
            DueDateTo = new DateOnly(2025, 1, 1)
        };

        await Assert.ThrowsAsync<ValidationException>(() => _service.GetAllAsync(filter));
        _repository.Verify(r => r.GetAllAsync(It.IsAny<TaskFilter>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Update ----------

    [Fact]
    public async Task UpdateAsync_Existing_ShouldUpdateAndPersist()
    {
        var task = NewTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        var later = Now.AddHours(2);
        _timeProvider.SetUtcNow(later);

        var request = new UpdateTaskRequest
        {
            Title = "Atualizada",
            Description = "Nova descrição",
            DueDate = new DateOnly(2025, 5, 1),
            Status = TaskItemStatus.Concluida
        };

        var result = await _service.UpdateAsync(task.Id, request);

        Assert.Equal("Atualizada", result.Title);
        Assert.Equal("Nova descrição", result.Description);
        Assert.Equal(request.DueDate, result.DueDate);
        Assert.Equal(TaskItemStatus.Concluida, result.Status);
        Assert.Equal(later.UtcDateTime, result.UpdatedAt);
        _repository.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ShouldThrowNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var request = new UpdateTaskRequest { Title = "X", Status = TaskItemStatus.Pendente };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), request));
        _repository.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ShouldThrowValidationBeforeQueryingRepository()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), new UpdateTaskRequest { Title = "" }));

        _repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Delete ----------

    [Fact]
    public async Task DeleteAsync_Existing_ShouldDelete()
    {
        var task = NewTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        await _service.DeleteAsync(task.Id);

        _repository.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ShouldThrowNotFound()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid()));
        _repository.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
