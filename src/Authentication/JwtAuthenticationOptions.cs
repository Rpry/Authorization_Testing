using Microsoft.AspNetCore.Authentication;

namespace Authorization.Samples.Authentication
{
    public class JwtSchemeOptions : AuthenticationSchemeOptions
    {
        public string Secret { get; set; }
    }
}