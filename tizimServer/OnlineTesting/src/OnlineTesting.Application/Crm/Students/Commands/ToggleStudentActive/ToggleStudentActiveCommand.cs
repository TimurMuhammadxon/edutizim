using MediatR;

namespace OnlineTesting.Application.Crm.Students.Commands.ToggleStudentActive;

public record ToggleStudentActiveCommand(Guid Id, bool IsActive) : IRequest;
