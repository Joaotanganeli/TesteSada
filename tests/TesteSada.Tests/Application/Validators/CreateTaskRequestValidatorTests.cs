using TesteSada.Application.DTOs;
using TesteSada.Application.Validators;
using TesteSada.Domain.Enums;
using TesteSada.Tests.Helpers;

namespace TesteSada.Tests.Application.Validators;

public class CreateTaskRequestValidatorTests
{
    private static readonly DateOnly Today = new(2025, 1, 10);
    private readonly CreateTaskRequestValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2025, 1, 10, 15, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void ValidRequest_ShouldPass()
    {
        var request = new CreateTaskRequest
        {
            Title = "Tarefa",
            Description = "Descrição",
            DueDate = Today,
            Status = TaskItemStatus.Pendente
        };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void OnlyTitle_ShouldPass()
    {
        Assert.True(_validator.Validate(new CreateTaskRequest { Title = "Tarefa" }).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTitle_ShouldFail(string title)
    {
        var result = _validator.Validate(new CreateTaskRequest { Title = title });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Title));
    }

    [Fact]
    public void TitleTooLong_ShouldFail()
    {
        var request = new CreateTaskRequest { Title = new string('a', TaskRules.TitleMaxLength + 1) };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Title));
    }

    [Fact]
    public void DescriptionTooLong_ShouldFail()
    {
        var request = new CreateTaskRequest
        {
            Title = "Tarefa",
            Description = new string('a', TaskRules.DescriptionMaxLength + 1)
        };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Description));
    }

    [Fact]
    public void DueDateInThePast_ShouldFail()
    {
        var request = new CreateTaskRequest { Title = "Tarefa", DueDate = Today.AddDays(-1) };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.DueDate));
    }

    [Fact]
    public void InvalidStatus_ShouldFail()
    {
        var request = new CreateTaskRequest { Title = "Tarefa", Status = (TaskItemStatus)99 };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Status));
    }
}
