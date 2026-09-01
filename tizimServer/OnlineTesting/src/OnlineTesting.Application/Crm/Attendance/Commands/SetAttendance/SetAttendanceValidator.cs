using FluentValidation;

namespace OnlineTesting.Application.Crm.Attendance.Commands.SetAttendance;

public class SetAttendanceValidator : AbstractValidator<SetAttendanceCommand>
{
    public SetAttendanceValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.LessonDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Cannot mark attendance for a future date.");
    }
}
