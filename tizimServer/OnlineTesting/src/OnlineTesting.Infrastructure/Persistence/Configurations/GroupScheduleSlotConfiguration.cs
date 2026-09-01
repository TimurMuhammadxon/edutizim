using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class GroupScheduleSlotConfiguration : IEntityTypeConfiguration<GroupScheduleSlot>
{
    public void Configure(EntityTypeBuilder<GroupScheduleSlot> builder)
    {
        builder.ToTable("group_schedule_slots");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(s => s.GroupId).HasColumnName("group_id").IsRequired();
        builder.Property(s => s.DayOfWeek).HasColumnName("day_of_week").HasConversion<int>().IsRequired();
        builder.Property(s => s.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(s => s.EndTime).HasColumnName("end_time").IsRequired();

        builder.HasIndex(s => s.OrganizationId).HasDatabaseName("ix_group_schedule_slots_organization_id");
        builder.HasIndex(s => s.GroupId).HasDatabaseName("ix_group_schedule_slots_group_id");
    }
}
