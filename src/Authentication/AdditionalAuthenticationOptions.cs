using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Authorization.Samples.Authentication
{
    public class AdditionalAuthenticationOptions : AuthenticationSchemeOptions
    {
        public IReadOnlyList<string> AdditionalClaimAttributes { get; set; }
    }
}