using FluentValidation;

namespace OnlineTesting.Application.Crm.Leads.Commands.CreateLead;

public class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Source).IsInEnum();
    }
}
