using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using TesteSada.Api.ErrorHandling;
using TesteSada.Application;
using TesteSada.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------- Camadas (Injeção de Dependência) ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Tratamento de erros ----------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------- Controllers / JSON ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        // Enums trafegam como texto ("Pendente") em vez de número (0).
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teste SADA - API de Gestão de Tarefas",
        Version = "v1",
        Description = "API RESTful para criar, listar, filtrar, editar e excluir tarefas."
    });

    // Inclui os comentários XML (///) dos projetos Api e Application na documentação.
    foreach (var assemblyName in new[] { Assembly.GetExecutingAssembly().GetName().Name, "TesteSada.Application" })
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ---------- Pipeline HTTP (a ordem importa) ----------
app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Teste SADA v1");
    options.DocumentTitle = "Teste SADA - Swagger";
});

// Frontend estático (wwwroot/index.html).
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

// Torna a classe Program visível para os testes de integração (WebApplicationFactory).
public partial class Program;
