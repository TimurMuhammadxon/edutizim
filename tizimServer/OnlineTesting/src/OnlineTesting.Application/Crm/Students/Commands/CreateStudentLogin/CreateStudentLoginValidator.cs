using FluentValidation;

namespace OnlineTesting.Application.Crm.Students.Commands.CreateStudentLogin;

public class CreateStudentLoginValidator : AbstractValidator<CreateStudentLoginCommand>
{
    public CreateStudentLoginValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
    }
}
