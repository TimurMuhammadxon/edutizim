using FluentValidation;

namespace OnlineTesting.Application.Crm.Groups.Commands.SetGroupSchedule;

public class SetGroupScheduleValidator : AbstractValidator<SetGroupScheduleCommand>
{
    public SetGroupScheduleValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.EndTime).GreaterThan(s => s.StartTime)
                .WithMessage("End time must be after start time.");
        });
    }
}
