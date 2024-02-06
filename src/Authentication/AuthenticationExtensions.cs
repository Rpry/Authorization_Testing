using Microsoft.AspNetCore.Authentication;

namespace Authorization.Samples.Authentication
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddCustomJwtToken(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<JwtSchemeOptions, JwtSchemeHandler>(
                AuthenticationSchemes.AppJwt,
                _ => { });
        }
    }
}