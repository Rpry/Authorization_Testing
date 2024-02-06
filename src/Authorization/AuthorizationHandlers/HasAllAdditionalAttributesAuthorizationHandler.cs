using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization.Samples.Authentication;
using Authorization.Samples.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Authorization.Samples.Authorization.AuthorizationHandlers
{
    public class
        HasAllAdditionalAttributesAuthorizationHandler : AuthorizationHandler<HasAllAdditionalAttributesRequirement>
    {
        private IReadOnlyList<string> attributes;

        
        public HasAllAdditionalAttributesAuthorizationHandler(IOptions<AdditionalAuthenticationOptions> options)
        {
            attributes = options.Value.AdditionalClaimAttributes;
        }
        

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            HasAllAdditionalAttributesRequirement requirement)
        {
            if (context.User.Claims.Join(attributes, c => c.Type, c => c, (a, b) => true).Count() ==
                attributes.Count)
                context.Succeed(requirement);
        }
    }
}