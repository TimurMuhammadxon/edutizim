using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Infrastructure.Persistence;

namespace OnlineTesting.Tests.Common;

/// Builds real ApplicationDbContext instances (same OnModelCreating, same tenant
/// query filter) against an InMemory database, so tests exercise the actual EF
/// Core filter logic rather than a hand-rolled fake.
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string databaseName, ICurrentUser currentUser)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options, currentUser);
    }
}
