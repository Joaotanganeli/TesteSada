namespace TesteSada.Domain.Enums;

/// <summary>
/// Situação atual de uma tarefa.
/// </summary>
public enum TaskItemStatus
{
    /// <summary>Tarefa ainda não iniciada.</summary>
    Pendente = 0,

    /// <summary>Tarefa em andamento.</summary>
    EmProgresso = 1,

    /// <summary>Tarefa finalizada.</summary>
    Concluida = 2
}
