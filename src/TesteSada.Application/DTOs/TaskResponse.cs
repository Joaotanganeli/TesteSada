using TesteSada.Domain.Enums;

namespace TesteSada.Application.DTOs;

/// <summary>
/// Representação de uma tarefa devolvida pela API.
/// </summary>
/// <param name="Id">Código único da tarefa.</param>
/// <param name="Title">Título.</param>
/// <param name="Description">Descrição.</param>
/// <param name="DueDate">Data de vencimento.</param>
/// <param name="Status">Status atual.</param>
/// <param name="CreatedAt">Data/hora (UTC) de criação.</param>
/// <param name="UpdatedAt">Data/hora (UTC) da última alteração.</param>
public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
