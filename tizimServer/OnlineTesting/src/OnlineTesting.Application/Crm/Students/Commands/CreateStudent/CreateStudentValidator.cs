using FluentValidation;

namespace OnlineTesting.Application.Crm.Students.Commands.CreateStudent;

public class CreateStudentValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
