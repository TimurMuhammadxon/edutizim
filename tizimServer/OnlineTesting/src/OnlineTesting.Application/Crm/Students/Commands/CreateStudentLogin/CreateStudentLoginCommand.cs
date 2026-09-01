using MediatR;

namespace OnlineTesting.Application.Crm.Students.Commands.CreateStudentLogin;

public record CreateStudentLoginCommand(Guid StudentId, string Password) : IRequest<Guid>;
