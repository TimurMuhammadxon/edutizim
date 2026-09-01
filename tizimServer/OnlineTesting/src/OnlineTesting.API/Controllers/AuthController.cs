using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using OnlineTesting.API.Services;
using OnlineTesting.Application.Auth.Commands.Login;
using OnlineTesting.Application.Auth.Commands.Logout;
using OnlineTesting.Application.Auth.Commands.Refresh;
using OnlineTesting.Application.Auth.Commands.Register;
using OnlineTesting.Application.Auth.Commands.GoogleLogin;
using OnlineTesting.Application.Auth.Commands.TelegramLogin;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Infrastructure.Authentication;

namespace OnlineTesting.API.Controllers;

[ApiController]
[Route("auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly JwtOptions _jwtOptions;

    public AuthController(ISender mediator, IOptions<JwtOptions> jwtOptions)
    {
        _mediator = mediator;
        _jwtOptions = jwtOptions.Value;
    }

    public record TelegramRequest(string InitData);
    public record GoogleRequest(string IdToken);

    /// What the client actually receives — the refresh token travels only via the
    /// httpOnly cookie set alongside this response, never in the JSON body.
    public record AccessTokenResponse(string AccessToken, int ExpiresIn);

    [AllowAnonymous]
    [HttpPost("register")]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("auth-strict")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> Login(
        [FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        RefreshTokenCookie.Set(Response, Request, result.RefreshToken, _jwtOptions);
        return Ok(new AccessTokenResponse(result.AccessToken, result.ExpiresIn));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [EnableRateLimiting("auth-normal")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> Refresh(CancellationToken ct)
    {
        var incoming = Request.Cookies[RefreshTokenCookie.Name];
        if (string.IsNullOrEmpty(incoming))
            throw new UnauthorizedException("Missing refresh token.");

        var result = await _mediator.Send(new RefreshCommand(incoming), ct);
        RefreshTokenCookie.Set(Response, Request, result.RefreshToken, _jwtOptions);
        return Ok(new AccessTokenResponse(result.AccessToken, result.ExpiresIn));
    }

    [AllowAnonymous]
    [HttpPost("telegram")]
    [EnableRateLimiting("auth-normal")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> Telegram(
        [FromBody] TelegramRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new TelegramLoginCommand(body.InitData), ct);
        RefreshTokenCookie.Set(Response, Request, result.RefreshToken, _jwtOptions);
        return Ok(new AccessTokenResponse(result.AccessToken, result.ExpiresIn));
    }

    [AllowAnonymous]
    [HttpPost("google")]
    [EnableRateLimiting("auth-normal")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenResponse>> Google(
        [FromBody] GoogleRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new GoogleLoginCommand(body.IdToken), ct);
        RefreshTokenCookie.Set(Response, Request, result.RefreshToken, _jwtOptions);
        return Ok(new AccessTokenResponse(result.AccessToken, result.ExpiresIn));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var incoming = Request.Cookies[RefreshTokenCookie.Name];
        if (!string.IsNullOrEmpty(incoming))
            await _mediator.Send(new LogoutCommand(incoming), ct);

        RefreshTokenCookie.Clear(Response);
        return NoContent();
    }
}
