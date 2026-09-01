using FluentValidation;

namespace OnlineTesting.Application.Crm.Groups.Commands.CreateGroup;

public class CreateGroupValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
