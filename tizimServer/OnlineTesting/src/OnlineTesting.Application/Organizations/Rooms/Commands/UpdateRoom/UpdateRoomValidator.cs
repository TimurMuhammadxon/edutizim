using FluentValidation;

namespace OnlineTesting.Application.Organizations.Rooms.Commands.UpdateRoom;

public class UpdateRoomValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}
