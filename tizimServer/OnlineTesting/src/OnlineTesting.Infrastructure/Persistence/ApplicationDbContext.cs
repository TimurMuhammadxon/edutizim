using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Common;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Organizations;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<CrmTask> CrmTasks => Set<CrmTask>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupStudent> GroupStudents => Set<GroupStudent>();
    public DbSet<GroupScheduleSlot> GroupScheduleSlots => Set<GroupScheduleSlot>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Payment> TuitionPayments => Set<Payment>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType)) continue;

            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(BuildTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);
            var filter = method.Invoke(this, null);
            entityType.SetQueryFilter((LambdaExpression)filter!);
        }

        base.OnModelCreating(modelBuilder);
    }

    // Platform-level roles (Owner/SuperAdmin) bypass tenant filtering — cross-tenant visibility
    // for a future platform admin panel. No authenticated context (e.g. a background job) means
    // OrganizationId/Role are both null, so the filter falls through to "matches nothing" — a
    // safe default rather than an accidental cross-tenant leak.
    private bool BypassTenantFilter =>
        _currentUser.OrganizationId is null && (_currentUser.Role?.IsPlatformLevel() ?? false);

    private LambdaExpression BuildTenantFilter<TEntity>() where TEntity : class, ITenantScoped
    {
        Expression<Func<TEntity, bool>> filter = e =>
            BypassTenantFilter || e.OrganizationId == _currentUser.OrganizationId;
        return filter;
    }
}
