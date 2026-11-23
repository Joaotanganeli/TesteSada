using Microsoft.AspNetCore.Mvc;
using TesteSada.Application.DTOs;
using TesteSada.Application.Interfaces;

namespace TesteSada.Api.Controllers;

/// <summary>
/// Endpoints RESTful para gerenciamento de tarefas.
/// </summary>
/// <remarks>
/// O controller é "magro": apenas recebe a requisição, delega ao serviço e devolve o status HTTP.
/// Erros (validação / não encontrado) são tratados de forma centralizada pelo GlobalExceptionHandler.
/// </remarks>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Lista as tarefas, com filtros opcionais por status e/ou intervalo de data de vencimento.
    /// </summary>
    /// <param name="filter">Filtros opcionais.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Lista de tarefas (pode ser vazia).</response>
    /// <response code="400">Filtros inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery] TaskFilter filter,
        CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetAllAsync(filter, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Obtém uma tarefa pelo seu código único.
    /// </summary>
    /// <param name="id">Código único (GUID) da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <param name="request">Dados da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A tarefa criada, incluindo o código único gerado.</returns>
    /// <response code="201">Tarefa criada. O header Location aponta para o novo recurso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Atualiza todos os campos de uma tarefa existente.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <param name="request">Novos dados da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="200">Tarefa atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Exclui uma tarefa.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <response code="204">Tarefa excluída.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
