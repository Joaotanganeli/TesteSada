using TesteSada.Domain.Enums;

namespace TesteSada.Application.DTOs;

/// <summary>
/// Dados para criação de uma nova tarefa.
/// </summary>
public record CreateTaskRequest
{
    /// <summary>Título da tarefa (obrigatório, até 200 caracteres).</summary>
    /// <example>Estudar Entity Framework Core</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>Descrição detalhada (opcional, até 1000 caracteres).</summary>
    /// <example>Ler a documentação oficial sobre o provider InMemory.</example>
    public string? Description { get; init; }

    /// <summary>Data de vencimento (opcional, formato yyyy-MM-dd). Não pode estar no passado.</summary>
    /// <example>2030-12-31</example>
    public DateOnly? DueDate { get; init; }

    /// <summary>Status inicial: Pendente, EmProgresso ou Concluida. Padrão: Pendente.</summary>
    /// <example>Pendente</example>
    public TaskItemStatus Status { get; init; } = TaskItemStatus.Pendente;
}
