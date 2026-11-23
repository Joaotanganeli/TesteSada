using TesteSada.Application.DTOs;

namespace TesteSada.Application.Interfaces;

/// <summary>
/// Casos de uso (regras de negócio) de tarefas.
/// </summary>
public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskFilter filter, CancellationToken cancellationToken = default);
    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
