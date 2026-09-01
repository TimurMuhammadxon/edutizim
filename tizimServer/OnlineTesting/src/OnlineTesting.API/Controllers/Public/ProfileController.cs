using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineTesting.API.Controllers;
using OnlineTesting.API.Services;
using OnlineTesting.Application.Users.Commands.SetCredentials;
using OnlineTesting.Application.Users.Commands.UpdateProfile;
using OnlineTesting.Infrastructure.Authentication;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("users/me")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly JwtOptions _jwtOptions;

    public ProfileController(ISender mediator, IOptions<JwtOptions> jwtOptions)
    {
        _mediator = mediator;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPatch]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile(
        [FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("credentials")]
    [ProducesResponseType(typeof(AuthController.AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthController.AccessTokenResponse>> SetCredentials(
        [FromBody] SetCredentialsCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        RefreshTokenCookie.Set(Response, Request, result.RefreshToken, _jwtOptions);
        return Ok(new AuthController.AccessTokenResponse(result.AccessToken, result.ExpiresIn));
    }
}
