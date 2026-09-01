using MediatR;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.GroupStudents.Commands.SetMembershipStatus;

public record SetMembershipStatusCommand(Guid GroupId, Guid StudentId, GroupMembershipStatus Status) : IRequest;
