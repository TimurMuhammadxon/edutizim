using MediatR;

namespace OnlineTesting.Application.Crm.GroupStudents.Commands.SetDiscount;

public record SetDiscountCommand(Guid GroupId, Guid StudentId, decimal Price, DateOnly StartDate, DateOnly EndDate) : IRequest;
