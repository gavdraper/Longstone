using System.Security.Claims;
using Longstone.Domain.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using IAuthenticationService = Longstone.Domain.Auth.IAuthenticationService;

namespace Longstone.Web.Auth;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", (Delegate)HandleLoginAsync).DisableAntiforgery();
        app.MapPost("/api/auth/logout", (Delegate)HandleLogoutAsync).DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> HandleLoginAsync(HttpContext httpContext, IAuthenticationService authService)
    {
        var form = await httpContext.Request.ReadFormAsync();
        var username = form["username"].ToString();
        var password = form["password"].ToString();
        var returnUrl = form["returnUrl"].ToString();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Results.Redirect("/auth/login?error=Invalid+username+or+password.");
        }

        var result = await authService.ValidateCredentialsAsync(username, password);

        if (!result.Succeeded || result.User is null)
        {
            return Results.Redirect($"/auth/login?error={Uri.EscapeDataString(result.ErrorMessage ?? "Invalid username or password.")}");
        }

        var claims = BuildClaims(result.User);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var redirect = IsLocalUrl(returnUrl) ? returnUrl : "/";
        return Results.Redirect(redirect);
    }

    private static async Task<IResult> HandleLogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/auth/login");
    }

    private static List<Claim> BuildClaims(User user)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(LongstoneClaimTypes.FullName, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        ];
    }

    private static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Reject protocol-relative URLs (//evil.com)
        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            return false;
        }

        // Reject absolute URIs with a scheme (http://, https://, etc.)
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme != Uri.UriSchemeFile)
        {
            return false;
        }

        // Must start with / (relative path)
        return url.StartsWith('/');
    }
}
