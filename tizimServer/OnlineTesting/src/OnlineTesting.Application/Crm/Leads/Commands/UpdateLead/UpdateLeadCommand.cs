using MediatR;

namespace OnlineTesting.Application.Crm.Leads.Commands.UpdateLead;

public record UpdateLeadCommand(Guid Id, string FullName, string Phone, string? Email, string? Notes) : IRequest;
