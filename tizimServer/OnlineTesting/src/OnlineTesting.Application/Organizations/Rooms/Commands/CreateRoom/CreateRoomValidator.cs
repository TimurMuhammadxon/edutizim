using FluentValidation;

namespace OnlineTesting.Application.Organizations.Rooms.Commands.CreateRoom;

public class CreateRoomValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}
