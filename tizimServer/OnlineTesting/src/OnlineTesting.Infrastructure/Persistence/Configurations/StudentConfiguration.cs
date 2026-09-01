using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(s => s.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(s => s.LeadId).HasColumnName("lead_id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(s => s.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(s => s.Notes).HasColumnName("notes");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(s => s.OrganizationId).HasDatabaseName("ix_students_organization_id");
        builder.HasIndex(s => s.BranchId).HasDatabaseName("ix_students_branch_id");
        builder.HasIndex(s => s.LeadId).HasDatabaseName("ix_students_lead_id");
        builder.HasIndex(s => s.UserId).HasDatabaseName("ix_students_user_id");
    }
}
