using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Organizations;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ExternalLogin> ExternalLogins { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Lead> Leads { get; }
    DbSet<Student> Students { get; }
    DbSet<CrmTask> CrmTasks { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupStudent> GroupStudents { get; }
    DbSet<GroupScheduleSlot> GroupScheduleSlots { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<Payment> TuitionPayments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
