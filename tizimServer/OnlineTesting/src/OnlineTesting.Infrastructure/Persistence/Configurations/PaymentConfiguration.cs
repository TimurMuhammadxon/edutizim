using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("tuition_payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(p => p.GroupId).HasColumnName("group_id").IsRequired();
        builder.Property(p => p.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(p => p.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(p => p.PaidAt).HasColumnName("paid_at").IsRequired();
        builder.Property(p => p.ForMonth).HasColumnName("for_month").IsRequired();
        builder.Property(p => p.Method).HasColumnName("method").HasConversion<int>().IsRequired();
        builder.Property(p => p.Note).HasColumnName("note");
        builder.Property(p => p.RecordedByUserId).HasColumnName("recorded_by_user_id").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(p => p.OrganizationId).HasDatabaseName("ix_tuition_payments_organization_id");
        builder.HasIndex(p => p.GroupId).HasDatabaseName("ix_tuition_payments_group_id");
        builder.HasIndex(p => p.StudentId).HasDatabaseName("ix_tuition_payments_student_id");
    }
}
