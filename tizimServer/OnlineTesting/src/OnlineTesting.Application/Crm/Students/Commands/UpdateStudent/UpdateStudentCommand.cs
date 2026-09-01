using MediatR;

namespace OnlineTesting.Application.Crm.Students.Commands.UpdateStudent;

public record UpdateStudentCommand(Guid Id, string FullName, string Phone, string? Email, string? Notes) : IRequest;
