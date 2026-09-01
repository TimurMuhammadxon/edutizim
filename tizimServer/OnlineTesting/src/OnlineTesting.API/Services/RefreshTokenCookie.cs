using OnlineTesting.Infrastructure.Authentication;

namespace OnlineTesting.API.Services;

/// The refresh token never reaches JS: it travels only as an httpOnly cookie, scoped to
/// /auth so it isn't attached to every unrelated request. Secure mirrors the actual
/// request scheme (via ForwardedHeaders behind a reverse proxy) so it still works over
/// plain HTTP in local dev without needing separate dev/prod configuration.
public static class RefreshTokenCookie
{
    public const string Name = "refreshToken";

    public static void Set(HttpResponse response, HttpRequest request, string token, JwtOptions jwtOptions)
    {
        response.Cookies.Append(Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshTokenDays),
        });
    }

    public static void Clear(HttpResponse response)
    {
        response.Cookies.Delete(Name, new CookieOptions { Path = "/auth" });
    }
}
