using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Organizations;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(o => o.Slug).HasColumnName("slug").IsRequired().HasMaxLength(250);
        builder.Property(o => o.OwnerUserId).HasColumnName("owner_user_id");
        builder.Property(o => o.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(o => o.Slug)
            .IsUnique()
            .HasDatabaseName("ux_organizations_slug");
    }
}
