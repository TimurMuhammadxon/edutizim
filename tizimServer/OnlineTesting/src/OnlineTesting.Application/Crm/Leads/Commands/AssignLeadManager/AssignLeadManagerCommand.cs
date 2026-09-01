using MediatR;

namespace OnlineTesting.Application.Crm.Leads.Commands.AssignLeadManager;

public record AssignLeadManagerCommand(Guid Id, Guid? ManagerId) : IRequest;
