using TesteSada.Application.DTOs;
using TesteSada.Application.Validators;
using TesteSada.Domain.Enums;

namespace TesteSada.Tests.Application.Validators;

public class TaskFilterValidatorTests
{
    private readonly TaskFilterValidator _validator = new();

    [Fact]
    public void EmptyFilter_ShouldPass()
    {
        Assert.True(_validator.Validate(new TaskFilter()).IsValid);
    }

    [Fact]
    public void ValidRange_ShouldPass()
    {
        var filter = new TaskFilter
        {
            Status = TaskItemStatus.Pendente,
            DueDateFrom = new DateOnly(2025, 1, 1),
            DueDateTo = new DateOnly(2025, 1, 1)
        };

        Assert.True(_validator.Validate(filter).IsValid);
    }

    [Fact]
    public void FromAfterTo_ShouldFail()
    {
        var filter = new TaskFilter
        {
            DueDateFrom = new DateOnly(2025, 2, 1),
            DueDateTo = new DateOnly(2025, 1, 1)
        };

        Assert.False(_validator.Validate(filter).IsValid);
    }

    [Fact]
    public void InvalidStatus_ShouldFail()
    {
        Assert.False(_validator.Validate(new TaskFilter { Status = (TaskItemStatus)7 }).IsValid);
    }
}
