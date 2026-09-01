using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Organizations.Members.Commands.CreateStaffMember;

public class CreateStaffMemberHandler : IRequestHandler<CreateStaffMemberCommand, Guid>
{
    private const string PhoneConflictMessage = "A user with this phone number already exists.";

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _hasher;
    private readonly IDbExceptionInspector _dbInspector;

    public CreateStaffMemberHandler(
        IApplicationDbContext db, ICurrentUser currentUser, IPasswordHasher hasher, IDbExceptionInspector dbInspector)
    {
        _db = db;
        _currentUser = currentUser;
        _hasher = hasher;
        _dbInspector = dbInspector;
    }

    public async Task<Guid> Handle(CreateStaffMemberCommand request, CancellationToken ct)
    {
        var organizationId = _currentUser.OrganizationId
            ?? throw new UnauthorizedException("User does not belong to an organization.");

        var hash = await _hasher.HashAsync(request.Password, ct);
        var user = User.CreateOrgMember(
            organizationId, Role.Staff, request.Phone, hash, request.FirstName, request.LastName);

        _db.Users.Add(user);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_dbInspector.IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException(PhoneConflictMessage);
        }

        return user.Id;
    }
}
