using FluentValidation;
using TesteSada.Application.DTOs;

namespace TesteSada.Application.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(TaskRules.TitleMaxLength)
            .WithMessage($"O título deve ter no máximo {TaskRules.TitleMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(TaskRules.DescriptionMaxLength)
            .WithMessage($"A descrição deve ter no máximo {TaskRules.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido. Valores aceitos: Pendente, EmProgresso, Concluida.");

        RuleFor(x => x.DueDate)
            .Must(dueDate => dueDate >= DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .When(x => x.DueDate.HasValue)
            .WithMessage("A data de vencimento não pode estar no passado.");
    }
}
