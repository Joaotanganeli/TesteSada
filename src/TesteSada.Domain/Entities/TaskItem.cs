using TesteSada.Domain.Enums;

namespace TesteSada.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa uma tarefa.
/// Os setters são privados: o estado só muda pelos métodos da própria entidade (encapsulamento).
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Construtor exigido pelo Entity Framework Core.
    private TaskItem() { }

    public static TaskItem Create(
        string title,
        string? description,
        DateOnly? dueDate,
        TaskItemStatus status,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título é obrigatório.", nameof(title));

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = NormalizeDescription(description),
            DueDate = dueDate,
            Status = status,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string title,
        string? description,
        DateOnly? dueDate,
        TaskItemStatus status,
        DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("O título é obrigatório.", nameof(title));

        Title = title.Trim();
        Description = NormalizeDescription(description);
        DueDate = dueDate;
        Status = status;
        UpdatedAt = updatedAt;
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
