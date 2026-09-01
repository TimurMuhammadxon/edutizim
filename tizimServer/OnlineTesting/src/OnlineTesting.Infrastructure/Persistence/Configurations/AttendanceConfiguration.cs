using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("attendance");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(a => a.GroupId).HasColumnName("group_id").IsRequired();
        builder.Property(a => a.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(a => a.LessonDate).HasColumnName("lesson_date").IsRequired();
        builder.Property(a => a.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(a => a.MarkedByUserId).HasColumnName("marked_by_user_id").IsRequired();
        builder.Property(a => a.MarkedAt).HasColumnName("marked_at").IsRequired();

        builder.HasIndex(a => a.OrganizationId).HasDatabaseName("ix_attendance_organization_id");
        builder.HasIndex(a => new { a.GroupId, a.StudentId, a.LessonDate })
            .IsUnique()
            .HasDatabaseName("ux_attendance_group_student_date");
    }
}
