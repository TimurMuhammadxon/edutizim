using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Commands.ChangeLeadStage;

public record ChangeLeadStageCommand(Guid Id, LeadStage Stage, string? LostReason) : IRequest;
