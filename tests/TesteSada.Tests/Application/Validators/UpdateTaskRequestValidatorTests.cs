using TesteSada.Application.DTOs;
using TesteSada.Application.Validators;
using TesteSada.Domain.Enums;

namespace TesteSada.Tests.Application.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_WithPastDueDate_ShouldPass()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Tarefa",
            DueDate = new DateOnly(2000, 1, 1),
            Status = TaskItemStatus.Concluida
        };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void EmptyTitle_ShouldFail()
    {
        var result = _validator.Validate(new UpdateTaskRequest { Title = "" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskRequest.Title));
    }

    [Fact]
    public void InvalidStatus_ShouldFail()
    {
        var result = _validator.Validate(new UpdateTaskRequest { Title = "Tarefa", Status = (TaskItemStatus)42 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskRequest.Status));
    }

    [Fact]
    public void DescriptionTooLong_ShouldFail()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Tarefa",
            Description = new string('a', TaskRules.DescriptionMaxLength + 1)
        };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskRequest.Description));
    }
}
