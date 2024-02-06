using System.Linq;
using System.Threading.Tasks;
using Authorization.Samples.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Threading;
using StackExchange.Redis;

namespace Authorization.Samples.Authorization.AuthorizationHandlers
{
    public class OneTimeJwtTokenAuthorizationHandler : AuthorizationHandler<OneTimeJwtTokenRequirement>
    {
        private readonly AsyncLazy<IDatabaseAsync> _databaseLazy;

        public OneTimeJwtTokenAuthorizationHandler(AsyncLazy<IDatabaseAsync> databaseLazy)
        {
            _databaseLazy = databaseLazy;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OneTimeJwtTokenRequirement requirement)
        {
            var uid = context.User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            
            if (uid == null)
                context.Fail();

            var db = await _databaseLazy.GetValueAsync();
            
            if(await db.StringGetSetAsync(requirement.RedisKeyPrefix + uid, true) == (bool?)null)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}