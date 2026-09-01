using FluentValidation;

namespace OnlineTesting.Application.Crm.GroupStudents.Commands.SetDiscount;

public class SetDiscountValidator : AbstractValidator<SetDiscountCommand>
{
    public SetDiscountValidator()
    {
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}
