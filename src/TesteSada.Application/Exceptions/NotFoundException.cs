namespace TesteSada.Application.Exceptions;

/// <summary>
/// Lançada quando um recurso não é encontrado. É convertida em HTTP 404 pelo tratador global de erros.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} com id '{key}' não foi encontrada.")
    {
    }
}
