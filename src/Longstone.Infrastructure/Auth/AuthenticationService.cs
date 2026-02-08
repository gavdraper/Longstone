using System.Diagnostics;
using Longstone.Domain.Auth;
using Longstone.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Longstone.Infrastructure.Auth;

public sealed class AuthenticationService : IAuthenticationService
{
    private static readonly ActivitySource ActivitySource = new("Longstone");

    private readonly LongstoneDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        LongstoneDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthenticationResult> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        using var activity = ActivitySource.StartActivity("AuthenticationService.ValidateCredentials");
        activity?.SetTag("auth.username", username);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for non-existent user {Username}", username);
            activity?.SetTag("auth.result", "user_not_found");
            return new AuthenticationResult(false, null, "Invalid username or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for deactivated user {Username} (UserId: {UserId})", username, user.Id);
            activity?.SetTag("auth.result", "account_deactivated");
            activity?.SetTag("auth.user_id", user.Id.ToString());
            return new AuthenticationResult(false, null, "Invalid username or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Failed login attempt for user {Username} (UserId: {UserId}) - invalid password", username, user.Id);
            activity?.SetTag("auth.result", "invalid_password");
            activity?.SetTag("auth.user_id", user.Id.ToString());
            return new AuthenticationResult(false, null, "Invalid username or password.");
        }

        _logger.LogInformation("Successful login for user {Username} (UserId: {UserId}, Role: {Role})", username, user.Id, user.Role);
        activity?.SetTag("auth.result", "success");
        activity?.SetTag("auth.user_id", user.Id.ToString());
        activity?.SetTag("auth.role", user.Role.ToString());

        return new AuthenticationResult(true, user, null);
    }
}
