using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Organizations;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(b => b.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(b => b.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(b => b.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(b => b.OrganizationId).HasDatabaseName("ix_branches_organization_id");
    }
}
