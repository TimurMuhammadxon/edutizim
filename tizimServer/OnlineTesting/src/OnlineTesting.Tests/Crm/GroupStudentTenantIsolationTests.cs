using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Crm.GroupStudents.Commands.RemoveDiscount;
using OnlineTesting.Application.Crm.GroupStudents.Commands.SetDiscount;
using OnlineTesting.Application.Crm.GroupStudents.Commands.SetMembershipStatus;
using OnlineTesting.Application.Crm.Groups.Commands.RemoveStudentFromGroup;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Tests.Common;

namespace OnlineTesting.Tests.Crm;

/// Regression coverage for the cross-tenant IDOR fixed 2026-09-01: GroupStudent has
/// no OrganizationId of its own (the EF Core global tenant filter can't reach it), so
/// every handler that mutates a membership must first resolve the Group through the
/// tenant-filtered DbSet and 404 if it doesn't belong to the caller's org. These tests
/// seed a Group+Student+membership in org A, then call each handler as an org B user
/// with org A's real GroupId/StudentId — before the fix this silently succeeded.
public class GroupStudentTenantIsolationTests
{
    private sealed record Seed(Guid OrgAId, Guid OrgBId, Guid GroupId, Guid StudentId);

    private static Seed SeedOrgAMembership(string dbName)
    {
        var orgAId = Guid.NewGuid();
        var orgBId = Guid.NewGuid();

        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgAId));

        var group = Group.Create(orgAId, Guid.NewGuid(), "Backend group", price: 500_000);
        var student = Student.Create(orgAId, Guid.NewGuid(), "Aziz Aripov", "+998901112233");
        db.Groups.Add(group);
        db.Students.Add(student);
        db.GroupStudents.Add(GroupStudent.Create(group.Id, student.Id));
        db.SaveChanges();

        return new Seed(orgAId, orgBId, group.Id, student.Id);
    }

    [Fact]
    public async Task SetMembershipStatus_ForGroupInOtherOrg_ThrowsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgBId));
        var handler = new SetMembershipStatusHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new SetMembershipStatusCommand(seed.GroupId, seed.StudentId, GroupMembershipStatus.Frozen), default));
    }

    [Fact]
    public async Task SetMembershipStatus_ForGroupInOwnOrg_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgAId));
        var handler = new SetMembershipStatusHandler(db);

        await handler.Handle(new SetMembershipStatusCommand(seed.GroupId, seed.StudentId, GroupMembershipStatus.Frozen), default);

        var membership = db.GroupStudents.Single(gs => gs.GroupId == seed.GroupId && gs.StudentId == seed.StudentId);
        Assert.Equal(GroupMembershipStatus.Frozen, membership.Status);
    }

    [Fact]
    public async Task SetDiscount_ForGroupInOtherOrg_ThrowsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgBId));
        var handler = new SetDiscountHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new SetDiscountCommand(seed.GroupId, seed.StudentId, 100_000,
                DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))), default));
    }

    [Fact]
    public async Task SetDiscount_ForGroupInOwnOrg_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgAId));
        var handler = new SetDiscountHandler(db);
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);

        await handler.Handle(new SetDiscountCommand(seed.GroupId, seed.StudentId, 100_000, start, end), default);

        var membership = db.GroupStudents.Single(gs => gs.GroupId == seed.GroupId && gs.StudentId == seed.StudentId);
        Assert.Equal(100_000, membership.DiscountedPrice);
    }

    [Fact]
    public async Task RemoveDiscount_ForGroupInOtherOrg_ThrowsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgBId));
        var handler = new RemoveDiscountHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RemoveDiscountCommand(seed.GroupId, seed.StudentId), default));
    }

    [Fact]
    public async Task RemoveDiscount_ForGroupInOwnOrg_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using (var seedingDb = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgAId)))
        {
            var membership = seedingDb.GroupStudents.Single(gs => gs.GroupId == seed.GroupId && gs.StudentId == seed.StudentId);
            membership.SetDiscount(100_000, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
            seedingDb.SaveChanges();
        }

        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgAId));
        var handler = new RemoveDiscountHandler(db);

        await handler.Handle(new RemoveDiscountCommand(seed.GroupId, seed.StudentId), default);

        var updated = db.GroupStudents.Single(gs => gs.GroupId == seed.GroupId && gs.StudentId == seed.StudentId);
        Assert.Null(updated.DiscountedPrice);
    }

    [Fact]
    public async Task RemoveStudentFromGroup_ForGroupInOtherOrg_ThrowsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgBId));
        var handler = new RemoveStudentFromGroupHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RemoveStudentFromGroupCommand(seed.GroupId, seed.StudentId), default));
    }

    [Fact]
    public async Task RemoveStudentFromGroup_ForGroupInOwnOrg_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var seed = SeedOrgAMembership(dbName);
        using var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(seed.OrgAId));
        var handler = new RemoveStudentFromGroupHandler(db);

        await handler.Handle(new RemoveStudentFromGroupCommand(seed.GroupId, seed.StudentId), default);

        Assert.False(db.GroupStudents.Any(gs => gs.GroupId == seed.GroupId && gs.StudentId == seed.StudentId));
    }
}
