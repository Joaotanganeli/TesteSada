using FluentValidation;
using TesteSada.Application.DTOs;

namespace TesteSada.Application.Validators;

/// <summary>
/// Na edição não bloqueamos datas passadas: uma tarefa já atrasada precisa continuar editável.
/// </summary>
public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
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
    }
}
