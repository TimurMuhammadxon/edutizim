using FluentValidation;
using OnlineTesting.Domain.Crm;

namespace OnlineTesting.Application.Crm.Leads.Commands.ChangeLeadStage;

public class ChangeLeadStageValidator : AbstractValidator<ChangeLeadStageCommand>
{
    public ChangeLeadStageValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Stage).IsInEnum();
        RuleFor(x => x.Stage)
            .NotEqual(LeadStage.Converted)
            .WithMessage("Use the convert-to-student endpoint to mark a lead as converted.");
    }
}
