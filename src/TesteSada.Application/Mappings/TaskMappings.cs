using TesteSada.Application.DTOs;
using TesteSada.Domain.Entities;

namespace TesteSada.Application.Mappings;

/// <summary>
/// Conversão Entidade -> DTO. Mantém a entidade de domínio fora da camada de apresentação.
/// </summary>
public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.DueDate,
        task.Status,
        task.CreatedAt,
        task.UpdatedAt);
}
