using TesteSada.Domain.Enums;

namespace TesteSada.Application.DTOs;

/// <summary>
/// Dados para atualização completa de uma tarefa existente.
/// </summary>
public record UpdateTaskRequest
{
    /// <summary>Título da tarefa (obrigatório, até 200 caracteres).</summary>
    /// <example>Estudar Entity Framework Core</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>Descrição detalhada (opcional, até 1000 caracteres).</summary>
    /// <example>Revisar o capítulo sobre migrations.</example>
    public string? Description { get; init; }

    /// <summary>Data de vencimento (opcional, formato yyyy-MM-dd).</summary>
    /// <example>2030-12-31</example>
    public DateOnly? DueDate { get; init; }

    /// <summary>Status: Pendente, EmProgresso ou Concluida.</summary>
    /// <example>EmProgresso</example>
    public TaskItemStatus Status { get; init; }
}
