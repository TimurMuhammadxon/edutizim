using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Crm.Students.Commands.CreateStudentLogin;

public class CreateStudentLoginHandler : IRequestHandler<CreateStudentLoginCommand, Guid>
{
    private const string PhoneConflictMessage = "A user with this phone number already exists.";

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IDbExceptionInspector _dbInspector;

    public CreateStudentLoginHandler(IApplicationDbContext db, IPasswordHasher hasher, IDbExceptionInspector dbInspector)
    {
        _db = db;
        _hasher = hasher;
        _dbInspector = dbInspector;
    }

    public async Task<Guid> Handle(CreateStudentLoginCommand request, CancellationToken ct)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, ct)
            ?? throw new NotFoundException($"Student '{request.StudentId}' not found.");

        if (student.UserId is not null)
            throw new ConflictException("Student already has a login.");

        var (firstName, lastName) = SplitName(student.FullName);
        var hash = await _hasher.HashAsync(request.Password, ct);
        var user = User.CreateOrgMember(student.OrganizationId, Role.Student, student.Phone, hash, firstName, lastName);

        student.LinkUser(user.Id);
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

    private static (string? FirstName, string? LastName) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => (null, null),
            1 => (parts[0], null),
            _ => (parts[0], parts[1])
        };
    }
}
