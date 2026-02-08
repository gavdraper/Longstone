namespace Longstone.Domain.Auth;

public record AuthenticationResult(bool Succeeded, User? User, string? ErrorMessage);

public interface IAuthenticationService
{
    Task<AuthenticationResult> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
}
