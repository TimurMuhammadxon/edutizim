using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class GroupStudentConfiguration : IEntityTypeConfiguration<GroupStudent>
{
    public void Configure(EntityTypeBuilder<GroupStudent> builder)
    {
        builder.ToTable("group_students");
        builder.HasKey(gs => new { gs.GroupId, gs.StudentId });

        builder.Property(gs => gs.GroupId).HasColumnName("group_id");
        builder.Property(gs => gs.StudentId).HasColumnName("student_id");
        builder.Property(gs => gs.JoinedAt).HasColumnName("joined_at").IsRequired();
        builder.Property(gs => gs.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(gs => gs.ActivatedAt).HasColumnName("activated_at");
        builder.Property(gs => gs.FrozenAt).HasColumnName("frozen_at");
        builder.Property(gs => gs.DiscountedPrice).HasColumnName("discounted_price").HasColumnType("numeric(18,2)");
        builder.Property(gs => gs.DiscountStartDate).HasColumnName("discount_start_date");
        builder.Property(gs => gs.DiscountEndDate).HasColumnName("discount_end_date");

        builder.HasIndex(gs => gs.StudentId).HasDatabaseName("ix_group_students_student_id");
    }
}
