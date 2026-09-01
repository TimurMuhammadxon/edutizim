using FluentValidation;

namespace OnlineTesting.Application.Crm.Groups.Commands.UpdateGroup;

public class UpdateGroupValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
