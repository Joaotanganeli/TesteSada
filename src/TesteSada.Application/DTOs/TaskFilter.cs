using TesteSada.Domain.Enums;

namespace TesteSada.Application.DTOs;

/// <summary>
/// Filtros opcionais para a listagem de tarefas. Todos podem ser combinados.
/// </summary>
public record TaskFilter
{
    /// <summary>Filtra pelo status (Pendente, EmProgresso, Concluida).</summary>
    public TaskItemStatus? Status { get; init; }

    /// <summary>Retorna tarefas com vencimento a partir desta data (inclusive).</summary>
    public DateOnly? DueDateFrom { get; init; }

    /// <summary>Retorna tarefas com vencimento até esta data (inclusive).</summary>
    public DateOnly? DueDateTo { get; init; }
}
