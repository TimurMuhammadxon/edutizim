using FluentValidation;

namespace OnlineTesting.Application.Organizations.Branches.Commands.CreateBranch;

public class CreateBranchValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}
