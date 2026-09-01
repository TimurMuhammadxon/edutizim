using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(g => g.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(g => g.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasColumnName("description");
        builder.Property(g => g.Price).HasColumnName("price").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(g => g.TeacherId).HasColumnName("teacher_id");
        builder.Property(g => g.RoomId).HasColumnName("room_id");
        builder.Property(g => g.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(g => g.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(g => g.OrganizationId).HasDatabaseName("ix_groups_organization_id");
        builder.HasIndex(g => g.BranchId).HasDatabaseName("ix_groups_branch_id");
        builder.HasIndex(g => g.TeacherId).HasDatabaseName("ix_groups_teacher_id");
        builder.HasIndex(g => g.RoomId).HasDatabaseName("ix_groups_room_id");
    }
}
