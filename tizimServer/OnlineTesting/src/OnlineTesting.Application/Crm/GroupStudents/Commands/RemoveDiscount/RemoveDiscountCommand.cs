using MediatR;

namespace OnlineTesting.Application.Crm.GroupStudents.Commands.RemoveDiscount;

public record RemoveDiscountCommand(Guid GroupId, Guid StudentId) : IRequest;
