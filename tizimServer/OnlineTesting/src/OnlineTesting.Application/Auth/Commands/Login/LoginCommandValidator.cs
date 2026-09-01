using FluentValidation;

namespace OnlineTesting.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}