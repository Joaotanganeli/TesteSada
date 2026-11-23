namespace TesteSada.Application.Validators;

/// <summary>
/// Limites compartilhados entre os validadores (evita "números mágicos" espalhados).
/// </summary>
public static class TaskRules
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 1000;
}
