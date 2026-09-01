using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class CrmTaskConfiguration : IEntityTypeConfiguration<CrmTask>
{
    public void Configure(EntityTypeBuilder<CrmTask> builder)
    {
        builder.ToTable("crm_tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(t => t.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description");
        builder.Property(t => t.DueAt).HasColumnName("due_at").IsRequired();
        builder.Property(t => t.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(t => t.AssignedToUserId).HasColumnName("assigned_to_user_id").IsRequired();
        builder.Property(t => t.LeadId).HasColumnName("lead_id");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.CompletedAt).HasColumnName("completed_at");

        builder.HasIndex(t => t.OrganizationId).HasDatabaseName("ix_crm_tasks_organization_id");
        builder.HasIndex(t => t.AssignedToUserId).HasDatabaseName("ix_crm_tasks_assigned_to_user_id");
        builder.HasIndex(t => t.LeadId).HasDatabaseName("ix_crm_tasks_lead_id");
        builder.HasIndex(t => t.DueAt).HasDatabaseName("ix_crm_tasks_due_at");
    }
}
