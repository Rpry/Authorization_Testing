using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Samples.Authentication
{
    public class JwtSchemeHandler : AuthenticationHandler<JwtSchemeOptions>
    {
        private string _secret;

        public JwtSchemeHandler(
            IOptionsMonitor<JwtSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _secret = options.CurrentValue.Secret;
        }

        private (string header, string payload, string signature) ParseToken(string token)
        {
            var parts = token.Split('.');

            return (parts[0], parts[1], parts[2]);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorization) ||
                !authorization[0].StartsWith("Bearer "))
                return AuthenticateResult.NoResult();

            var token = authorization[0]["Bearer ".Length..];
            var (header, payload, signature) = ParseToken(token);
            var handler = new JwtSecurityTokenHandler();

            var bytesToSign = Encoding.UTF8.GetBytes($"{header}.{payload}");
            var alg = new HMACSHA256(Convert.FromBase64String(_secret));
            var calculatedSignature = Base64UrlEncode(alg.ComputeHash(bytesToSign));

            if (calculatedSignature != signature)
                return AuthenticateResult.Fail("Invalid credentials");

            var jwtToken = handler.ReadJwtToken(token);

            // JWT токен может содержать в себе любую информацию, которую мы в него положили при формировании.
            // Забираем данные пользователя из токена и аутентифицируем пользователя.
            var claims = jwtToken.Claims;

            var identity = new ClaimsIdentity(claims, this.Scheme.Name);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=') // Remove any trailing '='s
                .Replace('+', '-') // 62nd char of encoding
                .Replace('/', '_'); // 63rd char of encoding
        }
    }
}