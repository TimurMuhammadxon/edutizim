using MediatR;
using OnlineTesting.Application.Crm.Leads.Queries.GetLeads;

namespace OnlineTesting.Application.Crm.Leads.Queries.GetLeadById;

public record GetLeadByIdQuery(Guid Id) : IRequest<LeadDto>;
