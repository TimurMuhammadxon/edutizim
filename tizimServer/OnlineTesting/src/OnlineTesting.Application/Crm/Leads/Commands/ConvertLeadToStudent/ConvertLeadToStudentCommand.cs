using MediatR;

namespace OnlineTesting.Application.Crm.Leads.Commands.ConvertLeadToStudent;

public record ConvertLeadToStudentCommand(Guid Id) : IRequest<Guid>;
