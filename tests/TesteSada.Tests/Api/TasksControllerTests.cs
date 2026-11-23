using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TesteSada.Application.DTOs;
using TesteSada.Domain.Enums;

namespace TesteSada.Tests.Api;

/// <summary>
/// Testes de integração: fazem requisições HTTP reais à API e conferem os status codes.
/// </summary>
public class TasksControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public TasksControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<TaskResponse> CreateTaskAsync(string title, TaskItemStatus status = TaskItemStatus.Pendente, DateOnly? dueDate = null)
    {
        var response = await _client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest { Title = title, Status = status, DueDate = dueDate }, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions))!;
    }

    [Fact]
    public async Task Post_Valid_ShouldReturn201WithLocationAndId()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest { Title = "Nova", Status = TaskItemStatus.EmProgresso }, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        Assert.NotEqual(Guid.Empty, body!.Id);
        Assert.Equal(TaskItemStatus.EmProgresso, body.Status);
        Assert.EndsWith($"/api/tasks/{body.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_StatusAsText_ShouldBeAccepted()
    {
        var response = await _client.PostAsync("/api/tasks", JsonContent.Create(
            new { title = "Texto", status = "Concluida" }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Concluida\"", json);
    }

    [Fact]
    public async Task Post_WithoutTitle_ShouldReturn400WithErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.True(problem!.Errors.ContainsKey("Title"));
    }

    [Fact]
    public async Task Post_InvalidStatusText_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "X", status = "Inexistente" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Existing_ShouldReturn200()
    {
        var created = await CreateTaskAsync("Buscar");

        var response = await _client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        Assert.Equal("Buscar", body!.Title);
    }

    [Fact]
    public async Task GetById_Unknown_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithFilters_ShouldReturnOnlyMatchingTasks()
    {
        var date = new DateOnly(2099, 6, 15);
        var match = await CreateTaskAsync("Filtrada", TaskItemStatus.Concluida, date);
        await CreateTaskAsync("Outra data", TaskItemStatus.Concluida, date.AddDays(1));
        await CreateTaskAsync("Outro status", TaskItemStatus.Pendente, date);

        var response = await _client.GetAsync("/api/tasks?status=Concluida&dueDateFrom=2099-06-15&dueDateTo=2099-06-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>(JsonOptions);
        Assert.Equal(match.Id, Assert.Single(tasks!).Id);
    }

    [Fact]
    public async Task GetAll_InvalidRange_ShouldReturn400()
    {
        var response = await _client.GetAsync("/api/tasks?dueDateFrom=2025-02-01&dueDateTo=2025-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_Existing_ShouldReturn200AndUpdate()
    {
        var created = await CreateTaskAsync("Antes");

        var response = await _client.PutAsJsonAsync($"/api/tasks/{created.Id}",
            new UpdateTaskRequest { Title = "Depois", Status = TaskItemStatus.Concluida }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        Assert.Equal("Depois", body!.Title);
        Assert.Equal(TaskItemStatus.Concluida, body.Status);
        Assert.NotNull(body.UpdatedAt);
    }

    [Fact]
    public async Task Put_Unknown_ShouldReturn404()
    {
        var response = await _client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}",
            new UpdateTaskRequest { Title = "X", Status = TaskItemStatus.Pendente }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_Invalid_ShouldReturn400()
    {
        var created = await CreateTaskAsync("Valida");

        var response = await _client.PutAsJsonAsync($"/api/tasks/{created.Id}", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Existing_ShouldReturn204ThenGetReturns404()
    {
        var created = await CreateTaskAsync("Excluir");

        var delete = await _client.DeleteAsync($"/api/tasks/{created.Id}");
        var get = await _client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_Unknown_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_ShouldBeAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Frontend_ShouldBeServedAtRoot()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Teste SADA", await response.Content.ReadAsStringAsync());
    }
}
