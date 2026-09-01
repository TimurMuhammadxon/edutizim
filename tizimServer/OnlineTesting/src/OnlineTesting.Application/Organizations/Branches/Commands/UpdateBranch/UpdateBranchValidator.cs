using FluentValidation;

namespace OnlineTesting.Application.Organizations.Branches.Commands.UpdateBranch;

public class UpdateBranchValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}
