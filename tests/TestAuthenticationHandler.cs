using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Authorization.Tests
{
    class TestAuthenticationHandler : IAuthenticationHandler
    {
        private static Dictionary<string, AuthenticateResult> _authenticateResults = new();
        private string _scheme;
        private HttpContext _context;

        public TestAuthenticationHandler(string scheme, HttpContext context)
        {
            _scheme = scheme;
            _context = context;
        }

        public async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            return _authenticateResults.TryGetValue(_scheme, out var result) ? result : AuthenticateResult.NoResult();
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }

        public static void ClearSetups() => _authenticateResults.Clear();

        public static void SetupSuccess(string scheme, params Claim[] claims)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, nameof(TestAuthenticationHandler)));
            _authenticateResults[scheme] = AuthenticateResult.Success(new AuthenticationTicket(user, scheme));
        }

        public static void SetupFail(string scheme, string message)
        {
            _authenticateResults[scheme] = AuthenticateResult.Fail(message);
        }
    }
}