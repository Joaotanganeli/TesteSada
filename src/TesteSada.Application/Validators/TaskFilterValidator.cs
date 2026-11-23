using FluentValidation;
using TesteSada.Application.DTOs;

namespace TesteSada.Application.Validators;

public class TaskFilterValidator : AbstractValidator<TaskFilter>
{
    public TaskFilterValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Status inválido. Valores aceitos: Pendente, EmProgresso, Concluida.");

        RuleFor(x => x)
            .Must(f => f.DueDateFrom <= f.DueDateTo)
            .When(f => f.DueDateFrom.HasValue && f.DueDateTo.HasValue)
            .WithName("DueDateFrom")
            .WithMessage("A data inicial (dueDateFrom) deve ser menor ou igual à data final (dueDateTo).");
    }
}
