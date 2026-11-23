using TesteSada.Domain.Entities;
using TesteSada.Domain.Enums;

namespace TesteSada.Tests.Domain;

public class TaskItemTests
{
    private static readonly DateTime Now = new(2025, 1, 10, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldGenerateUniqueIdAndSetProperties()
    {
        var dueDate = new DateOnly(2025, 2, 1);

        var task = TaskItem.Create("  Título  ", "  Descrição  ", dueDate, TaskItemStatus.EmProgresso, Now);
        var other = TaskItem.Create("Outra", null, null, TaskItemStatus.Pendente, Now);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.NotEqual(task.Id, other.Id);
        Assert.Equal("Título", task.Title);
        Assert.Equal("Descrição", task.Description);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(TaskItemStatus.EmProgresso, task.Status);
        Assert.Equal(Now, task.CreatedAt);
        Assert.Null(task.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithoutTitle_ShouldThrow(string? title)
    {
        Assert.Throws<ArgumentException>(() =>
            TaskItem.Create(title!, null, null, TaskItemStatus.Pendente, Now));
    }

    [Fact]
    public void Create_WithBlankDescription_ShouldStoreNull()
    {
        var task = TaskItem.Create("Título", "   ", null, TaskItemStatus.Pendente, Now);

        Assert.Null(task.Description);
    }

    [Fact]
    public void Update_ShouldChangeAllFieldsAndSetUpdatedAt()
    {
        var task = TaskItem.Create("Título", "Desc", null, TaskItemStatus.Pendente, Now);
        var later = Now.AddHours(1);
        var newDueDate = new DateOnly(2025, 3, 1);

        task.Update("Novo título", null, newDueDate, TaskItemStatus.Concluida, later);

        Assert.Equal("Novo título", task.Title);
        Assert.Null(task.Description);
        Assert.Equal(newDueDate, task.DueDate);
        Assert.Equal(TaskItemStatus.Concluida, task.Status);
        Assert.Equal(later, task.UpdatedAt);
        Assert.Equal(Now, task.CreatedAt);
    }

    [Fact]
    public void Update_WithoutTitle_ShouldThrow()
    {
        var task = TaskItem.Create("Título", null, null, TaskItemStatus.Pendente, Now);

        Assert.Throws<ArgumentException>(() =>
            task.Update(" ", null, null, TaskItemStatus.Pendente, Now));
    }
}
