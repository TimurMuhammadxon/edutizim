using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Organizations;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(r => r.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(r => r.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(r => r.OrganizationId).HasDatabaseName("ix_rooms_organization_id");
        builder.HasIndex(r => r.BranchId).HasDatabaseName("ix_rooms_branch_id");
    }
}
