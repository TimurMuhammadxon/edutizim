using FluentValidation;

namespace OnlineTesting.Application.Crm.Attendance.Commands.SetAttendanceForDate;

public class SetAttendanceForDateValidator : AbstractValidator<SetAttendanceForDateCommand>
{
    public SetAttendanceForDateValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.LessonDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Cannot mark attendance for a future date.");
    }
}
