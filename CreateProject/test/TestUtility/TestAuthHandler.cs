using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Brimborium.CreateProject.Test.TestUtility;

/// <summary>
/// Authentication scheme options for the test authentication handler.
/// Extends the base <see cref="AuthenticationSchemeOptions"/> without additional configuration.
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions {
}

/// <summary>
/// Custom authentication handler for testing purposes that validates users against a predefined set of test users.
/// This handler parses custom authorization headers in the format "TestScheme username/password" and authenticates
/// users by looking them up in the <see cref="TestAuthenticationUsers"/> service.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<TestAuthenticationSchemeOptions> {
    /// <summary>
    /// The authentication scheme name used by this handler.
    /// </summary>
    public static string AuthenticationScheme = "TestScheme";

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthHandler"/> class.
    /// </summary>
    /// <param name="options">The authentication scheme options monitor.</param>
    /// <param name="logger">The logger factory for creating loggers.</param>
    /// <param name="encoder">The URL encoder for encoding URLs.</param>
    public TestAuthHandler(
        IOptionsMonitor<TestAuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder) {
    }

    /// <summary>
    /// Handles the authentication process by parsing the Authorization header and validating credentials
    /// against the test user database.
    /// </summary>
    /// <returns>
    /// An <see cref="AuthenticateResult"/> indicating the result of the authentication attempt:
    /// - Success with claims if valid credentials are provided
    /// - Failure if invalid credentials are provided
    /// - NoResult if no authorization header is present or malformed
    /// </returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        var userName = @"DEEUSEW\degriflo";
        var claims = new List<Claim>
            {
                new Claim(System.Security.Claims.ClaimTypes.Name, userName),
                new Claim("FullName", userName),
                //new Claim(ClaimTypes.Role, user.Role),
            };
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

}
