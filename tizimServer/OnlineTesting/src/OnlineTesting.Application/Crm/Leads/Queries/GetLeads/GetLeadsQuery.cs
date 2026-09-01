using MediatR;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Queries.GetLeads;

public record GetLeadsQuery(
    string? Search,
    ClientSource? Source,
    LeadStage? Stage,
    Guid? BranchId,
    Guid? AssignedManagerId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<LeadDto>>;

public record LeadDto(
    Guid Id,
    Guid BranchId,
    string FullName,
    string Phone,
    string? Email,
    string Source,
    string Stage,
    Guid? AssignedManagerId,
    string? Notes,
    string? LostReason,
    DateTime CreatedAt);
