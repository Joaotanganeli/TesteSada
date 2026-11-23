using Microsoft.EntityFrameworkCore;
using TesteSada.Application.DTOs;
using TesteSada.Domain.Entities;
using TesteSada.Domain.Enums;
using TesteSada.Infrastructure.Persistence;
using TesteSada.Infrastructure.Repositories;

namespace TesteSada.Tests.Infrastructure;

/// <summary>
/// Testa o repositório contra o provider InMemory (cada teste usa um banco isolado).
/// </summary>
public class TaskRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);
    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"repo-tests-{Guid.NewGuid()}")
            .Options;
        _context = new AppDbContext(options);
        _repository = new TaskRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<TaskItem> AddAsync(string title, TaskItemStatus status, DateOnly? dueDate)
    {
        var task = TaskItem.Create(title, null, dueDate, status, Now);
        await _repository.AddAsync(task);
        return task;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldReturnSameTask()
    {
        var task = await AddAsync("A", TaskItemStatus.Pendente, null);

        var found = await _repository.GetByIdAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal("A", found!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_Unknown_ShouldReturnNull()
    {
        Assert.Null(await _repository.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        await AddAsync("A", TaskItemStatus.Pendente, null);
        await AddAsync("B", TaskItemStatus.Concluida, null);

        var result = await _repository.GetAllAsync(new TaskFilter { Status = TaskItemStatus.Concluida });

        Assert.Single(result);
        Assert.Equal("B", result[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByDueDateRangeInclusive()
    {
        await AddAsync("Jan", TaskItemStatus.Pendente, new DateOnly(2025, 1, 15));
        await AddAsync("Fev", TaskItemStatus.Pendente, new DateOnly(2025, 2, 15));
        await AddAsync("Mar", TaskItemStatus.Pendente, new DateOnly(2025, 3, 15));
        await AddAsync("SemData", TaskItemStatus.Pendente, null);

        var result = await _repository.GetAllAsync(new TaskFilter
        {
            DueDateFrom = new DateOnly(2025, 2, 15),
            DueDateTo = new DateOnly(2025, 3, 15)
        });

        Assert.Equal(new[] { "Fev", "Mar" }, result.Select(t => t.Title));
    }

    [Fact]
    public async Task GetAllAsync_ShouldCombineStatusAndDate()
    {
        await AddAsync("A", TaskItemStatus.Pendente, new DateOnly(2025, 1, 15));
        await AddAsync("B", TaskItemStatus.EmProgresso, new DateOnly(2025, 1, 15));

        var result = await _repository.GetAllAsync(new TaskFilter
        {
            Status = TaskItemStatus.EmProgresso,
            DueDateFrom = new DateOnly(2025, 1, 15),
            DueDateTo = new DateOnly(2025, 1, 15)
        });

        Assert.Equal("B", Assert.Single(result).Title);
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByDueDateWithNullsLast()
    {
        await AddAsync("SemData", TaskItemStatus.Pendente, null);
        await AddAsync("Depois", TaskItemStatus.Pendente, new DateOnly(2025, 3, 1));
        await AddAsync("Antes", TaskItemStatus.Pendente, new DateOnly(2025, 1, 1));

        var result = await _repository.GetAllAsync(new TaskFilter());

        Assert.Equal(new[] { "Antes", "Depois", "SemData" }, result.Select(t => t.Title));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var task = await AddAsync("A", TaskItemStatus.Pendente, null);

        task.Update("A2", "desc", null, TaskItemStatus.Concluida, Now.AddHours(1));
        await _repository.UpdateAsync(task);
        _context.ChangeTracker.Clear();

        var found = await _repository.GetByIdAsync(task.Id);
        Assert.Equal("A2", found!.Title);
        Assert.Equal(TaskItemStatus.Concluida, found.Status);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        var task = await AddAsync("A", TaskItemStatus.Pendente, null);

        await _repository.DeleteAsync(task);

        Assert.Null(await _repository.GetByIdAsync(task.Id));
    }
}
