using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesteSada.Application.Validators;
using TesteSada.Domain.Entities;

namespace TesteSada.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento da entidade TaskItem (Fluent API). Mantém a entidade de domínio livre de atributos do EF.
/// </summary>
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Title).IsRequired().HasMaxLength(TaskRules.TitleMaxLength);
        builder.Property(t => t.Description).HasMaxLength(TaskRules.DescriptionMaxLength);
        builder.Property(t => t.Status).HasConversion<string>().IsRequired();
    }
}
