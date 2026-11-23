using FluentValidation;
using Microsoft.Extensions.Logging;
using TesteSada.Application.DTOs;
using TesteSada.Application.Exceptions;
using TesteSada.Application.Interfaces;
using TesteSada.Application.Mappings;
using TesteSada.Domain.Entities;

namespace TesteSada.Application.Services;

/// <summary>
/// Implementa as regras de negócio de tarefas.
/// Todas as dependências chegam pelo construtor (Injeção de Dependência) e são abstrações,
/// o que permite testar esta classe isoladamente com mocks.
/// </summary>
public class TaskService : ITaskService
{
    private const string ResourceName = "Tarefa";

    private readonly ITaskRepository _repository;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;
    private readonly IValidator<TaskFilter> _filterValidator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository repository,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        IValidator<TaskFilter> filterValidator,
        TimeProvider timeProvider,
        ILogger<TaskService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        await _filterValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var tasks = await _repository.GetAllAsync(filter, cancellationToken);
        return tasks.Select(t => t.ToResponse()).ToList();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);
        return task.ToResponse();
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            request.DueDate,
            request.Status,
            _timeProvider.GetUtcNow().UtcDateTime);

        await _repository.AddAsync(task, cancellationToken);
        _logger.LogInformation("Tarefa {TaskId} criada com sucesso.", task.Id);

        return task.ToResponse();
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = await GetExistingTaskAsync(id, cancellationToken);
        task.Update(
            request.Title,
            request.Description,
            request.DueDate,
            request.Status,
            _timeProvider.GetUtcNow().UtcDateTime);

        await _repository.UpdateAsync(task, cancellationToken);
        _logger.LogInformation("Tarefa {TaskId} atualizada com sucesso.", task.Id);

        return task.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingTaskAsync(id, cancellationToken);

        await _repository.DeleteAsync(task, cancellationToken);
        _logger.LogInformation("Tarefa {TaskId} excluída com sucesso.", id);
    }

    private async Task<TaskItem> GetExistingTaskAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            _logger.LogWarning("Tarefa {TaskId} não encontrada.", id);
            throw new NotFoundException(ResourceName, id);
        }

        return task;
    }
}
