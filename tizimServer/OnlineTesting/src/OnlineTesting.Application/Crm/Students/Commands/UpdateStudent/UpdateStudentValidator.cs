using FluentValidation;

namespace OnlineTesting.Application.Crm.Students.Commands.UpdateStudent;

public class UpdateStudentValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
