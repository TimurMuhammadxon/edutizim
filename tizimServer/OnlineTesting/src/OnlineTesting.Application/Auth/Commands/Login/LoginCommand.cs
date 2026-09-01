using MediatR;

namespace OnlineTesting.Application.Auth.Commands.Login;

public record LoginCommand(string Identifier, string Password) : IRequest<AuthResponse>;