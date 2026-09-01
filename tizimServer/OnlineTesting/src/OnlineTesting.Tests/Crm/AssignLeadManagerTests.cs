using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Crm.Leads.Commands.AssignLeadManager;
using OnlineTesting.Domain.Crm;
using OnlineTesting.Domain.Users;
using OnlineTesting.Tests.Common;

namespace OnlineTesting.Tests.Crm;

/// Regression coverage for a fix alongside the GroupStudent IDOR (2026-09-01):
/// AssignLeadManagerHandler previously accepted any GUID as ManagerId with no check
/// that it was a real user in the lead's own org.
public class AssignLeadManagerTests
{
    [Fact]
    public async Task AssignManager_ToUserInOtherOrg_ThrowsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        var orgAId = Guid.NewGuid();
        var orgBId = Guid.NewGuid();
        Guid leadId, otherOrgUserId;

        using (var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgAId)))
        {
            var lead = Lead.Create(orgAId, Guid.NewGuid(), "Aziz Aripov", "+998901112233", ClientSource.Instagram);
            db.Leads.Add(lead);
            leadId = lead.Id;
            db.SaveChanges();
        }

        using (var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgBId)))
        {
            var otherOrgUser = User.CreateOrgMember(orgBId, Role.Staff, "+998907778899", "hash");
            db.Users.Add(otherOrgUser);
            otherOrgUserId = otherOrgUser.Id;
            db.SaveChanges();
        }

        using var handlerDb = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgAId));
        var handler = new AssignLeadManagerHandler(handlerDb);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new AssignLeadManagerCommand(leadId, otherOrgUserId), default));
    }

    [Fact]
    public async Task AssignManager_ToUserInSameOrg_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var orgId = Guid.NewGuid();
        Guid leadId, managerId;

        using (var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgId)))
        {
            var lead = Lead.Create(orgId, Guid.NewGuid(), "Aziz Aripov", "+998901112233", ClientSource.Instagram);
            var manager = User.CreateOrgMember(orgId, Role.Staff, "+998907778899", "hash");
            db.Leads.Add(lead);
            db.Users.Add(manager);
            leadId = lead.Id;
            managerId = manager.Id;
            db.SaveChanges();
        }

        using var handlerDb = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgId));
        var handler = new AssignLeadManagerHandler(handlerDb);

        await handler.Handle(new AssignLeadManagerCommand(leadId, managerId), default);

        var updated = handlerDb.Leads.Single(l => l.Id == leadId);
        Assert.Equal(managerId, updated.AssignedManagerId);
    }

    [Fact]
    public async Task UnassignManager_WithNullManagerId_SkipsOwnershipCheck()
    {
        var dbName = Guid.NewGuid().ToString();
        var orgId = Guid.NewGuid();
        Guid leadId;

        using (var db = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgId)))
        {
            var manager = User.CreateOrgMember(orgId, Role.Staff, "+998907778899", "hash");
            db.Users.Add(manager);
            var lead = Lead.Create(orgId, Guid.NewGuid(), "Aziz Aripov", "+998901112233", ClientSource.Instagram, manager.Id);
            db.Leads.Add(lead);
            leadId = lead.Id;
            db.SaveChanges();
        }

        using var handlerDb = TestDbContextFactory.Create(dbName, FakeCurrentUser.ForOrg(orgId));
        var handler = new AssignLeadManagerHandler(handlerDb);

        await handler.Handle(new AssignLeadManagerCommand(leadId, null), default);

        var updated = handlerDb.Leads.Single(l => l.Id == leadId);
        Assert.Null(updated.AssignedManagerId);
    }
}
