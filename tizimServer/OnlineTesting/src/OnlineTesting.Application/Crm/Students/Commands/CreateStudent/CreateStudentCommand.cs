using MediatR;

namespace OnlineTesting.Application.Crm.Students.Commands.CreateStudent;

public record CreateStudentCommand(Guid BranchId, string FullName, string Phone, string? Email) : IRequest<Guid>;
