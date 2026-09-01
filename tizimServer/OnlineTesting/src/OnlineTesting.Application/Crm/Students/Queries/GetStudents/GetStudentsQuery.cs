using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Crm.Students.Queries.GetStudents;

public record GetStudentsQuery(
    string? Search,
    Guid? BranchId,
    Guid? GroupId,
    bool? IsActive,
    string? StudentStatus,
    string? FinancialStatus,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<StudentDto>>;

public record StudentDto(
    Guid Id,
    Guid BranchId,
    Guid? LeadId,
    Guid? UserId,
    string FullName,
    string Phone,
    string? Email,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt);
