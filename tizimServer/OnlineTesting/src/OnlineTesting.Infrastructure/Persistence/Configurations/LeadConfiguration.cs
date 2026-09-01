using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(l => l.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(l => l.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(l => l.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();
        builder.Property(l => l.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(l => l.Source).HasColumnName("source").HasConversion<int>().IsRequired();
        builder.Property(l => l.Stage).HasColumnName("stage").HasConversion<int>().IsRequired();
        builder.Property(l => l.AssignedManagerId).HasColumnName("assigned_manager_id");
        builder.Property(l => l.Notes).HasColumnName("notes");
        builder.Property(l => l.LostReason).HasColumnName("lost_reason").HasMaxLength(500);
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(l => l.OrganizationId).HasDatabaseName("ix_leads_organization_id");
        builder.HasIndex(l => l.BranchId).HasDatabaseName("ix_leads_branch_id");
        builder.HasIndex(l => l.AssignedManagerId).HasDatabaseName("ix_leads_assigned_manager_id");
        builder.HasIndex(l => l.Stage).HasDatabaseName("ix_leads_stage");
    }
}
