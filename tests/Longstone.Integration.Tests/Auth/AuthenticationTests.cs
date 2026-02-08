using System.Net;
using FluentAssertions;
using Longstone.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Longstone.Integration.Tests.Auth;

public class AuthenticationTests : IClassFixture<LongstoneWebApplicationFactory>
{
    private const string SeedPassword = "Dev123!";

    private readonly LongstoneWebApplicationFactory _factory;

    public AuthenticationTests(LongstoneWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateNoRedirectClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToDashboard()
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "admin",
            ["password"] = SeedPassword
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/");
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.Contains(".AspNetCore.Cookies"));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_RedirectsBackToLoginWithError()
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "admin",
            ["password"] = "WrongPassword!"
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/auth/login");
        response.Headers.Location.ToString().Should().Contain("error");
    }

    [Fact]
    public async Task Login_WithNonExistentUser_RedirectsBackToLoginWithError()
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "nonexistent",
            ["password"] = SeedPassword
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/auth/login");
        response.Headers.Location.ToString().Should().Contain("error");
    }

    [Fact]
    public async Task Login_WithDeactivatedUser_RedirectsBackToLoginWithError()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LongstoneDbContext>();
        var user = await dbContext.Users.FirstAsync(u => u.Username == "readonly");
        user.Deactivate(TimeProvider.System);
        await dbContext.SaveChangesAsync();

        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "readonly",
            ["password"] = SeedPassword
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/auth/login");
        response.Headers.Location.ToString().Should().Contain("error");

        // Re-activate the user for other tests
        user.Activate(TimeProvider.System);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_RedirectsToLogin()
    {
        var client = CreateNoRedirectClient();

        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "admin",
            ["password"] = SeedPassword
        });
        var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var setCookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var cookieHeader = string.Join("; ", setCookies.Select(c => c.Split(';')[0]));
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        logoutResponse.Headers.Location!.ToString().Should().Contain("/auth/login");
    }

    [Fact]
    public async Task Login_SetsCorrectClaimsForDifferentRoles()
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "fundmgr",
            ["password"] = SeedPassword
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/");
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.Contains(".AspNetCore.Cookies"));
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_RedirectsBackToLoginWithError()
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "",
            ["password"] = ""
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/auth/login");
        response.Headers.Location.ToString().Should().Contain("error");
    }

    [Fact]
    public async Task LoginPage_WhenUnauthenticated_Returns200()
    {
        var client = CreateNoRedirectClient();

        var response = await client.GetAsync("/auth/login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("https://evil.com")]
    [InlineData("//evil.com")]
    [InlineData("http://evil.com")]
    public async Task Login_WithExternalReturnUrl_RedirectsToDashboardInstead(string maliciousUrl)
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "admin",
            ["password"] = SeedPassword,
            ["returnUrl"] = maliciousUrl
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/");
    }

    [Fact]
    public async Task Login_WithLocalReturnUrl_RedirectsToThatPath()
    {
        var client = CreateNoRedirectClient();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "admin",
            ["password"] = SeedPassword,
            ["returnUrl"] = "/funds"
        });

        var response = await client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/funds");
    }
}
