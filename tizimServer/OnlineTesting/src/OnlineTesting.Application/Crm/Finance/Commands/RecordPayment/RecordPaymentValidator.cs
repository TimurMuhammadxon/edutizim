using FluentValidation;

namespace OnlineTesting.Application.Crm.Finance.Commands.RecordPayment;

public class RecordPaymentValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaidAt)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Payment date cannot be in the future.");
        RuleFor(x => x.Method).IsInEnum();
    }
}
