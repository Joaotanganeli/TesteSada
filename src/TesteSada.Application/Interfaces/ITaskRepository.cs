using TesteSada.Application.DTOs;
using TesteSada.Domain.Entities;

namespace TesteSada.Application.Interfaces;

/// <summary>
/// Abstração de acesso a dados (Repository Pattern).
/// A camada de aplicação depende desta interface, e não do EF Core (Inversão de Dependência).
/// </summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskFilter filter, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
}
