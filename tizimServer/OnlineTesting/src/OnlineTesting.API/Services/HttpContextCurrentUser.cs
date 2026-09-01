using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.API.Services;

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            // MapInboundClaims = false → ищем напрямую по 'sub'.
            // Fallback на NameIdentifier — на случай, если кто-то отключит наш конфиг.
            var raw =
                _accessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public Guid? OrganizationId
    {
        get
        {
            var raw = _accessor.HttpContext?.User.FindFirst(TenantClaims.OrganizationId)?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public Role? Role
    {
        get
        {
            var raw = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<Role>(raw, out var role) ? role : null;
        }
    }
}
