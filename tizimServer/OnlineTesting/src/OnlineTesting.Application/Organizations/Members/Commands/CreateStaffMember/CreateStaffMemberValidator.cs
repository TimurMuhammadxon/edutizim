using FluentValidation;

namespace OnlineTesting.Application.Organizations.Members.Commands.CreateStaffMember;

public class CreateStaffMemberValidator : AbstractValidator<CreateStaffMemberCommand>
{
    public CreateStaffMemberValidator()
    {
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
    }
}
