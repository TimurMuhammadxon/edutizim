using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Students.Queries.GetStudentById;

public record GetStudentByIdQuery(Guid Id) : IRequest<StudentDetailsDto>;

public record StudentDetailsDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    Guid? LeadId,
    string? LeadFullName,
    Guid? UserId,
    string FullName,
    string Phone,
    string? Email,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    DateTime? StartedAt,
    decimal TotalBalance,
    List<StudentGroupMembershipDto> Groups);

public record StudentGroupMembershipDto(
    Guid GroupId,
    string GroupName,
    string? TeacherName,
    GroupMembershipStatus Status,
    DateTime JoinedAt,
    DateTime? ActivatedAt,
    decimal EffectivePrice,
    decimal Balance,
    DateOnly? NextPaymentDueDate,
    int PresentCount,
    int AbsentCount);
