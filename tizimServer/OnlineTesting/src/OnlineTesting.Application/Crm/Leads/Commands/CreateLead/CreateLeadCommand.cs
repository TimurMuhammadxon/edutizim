using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Commands.CreateLead;

public record CreateLeadCommand(
    Guid BranchId,
    string FullName,
    string Phone,
    ClientSource Source,
    Guid? AssignedManagerId) : IRequest<Guid>;
