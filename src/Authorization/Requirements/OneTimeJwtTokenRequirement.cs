using Microsoft.AspNetCore.Authorization;

namespace Authorization.Samples.Authorization.Requirements
{
    public class OneTimeJwtTokenRequirement : IAuthorizationRequirement
    {
        public OneTimeJwtTokenRequirement(string redisKeyPrefix)
        {
            RedisKeyPrefix = redisKeyPrefix;
        }
        //IAuthorizationHandlerProvider
        public string RedisKeyPrefix { get; }
    }
}