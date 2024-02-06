using Microsoft.AspNetCore.Authorization;

namespace Authorization.Samples.Authorization.Requirements
{
    public class MinAgeRequirement : IAuthorizationRequirement
    {
        public MinAgeRequirement(int minAge)
        {
            MinAge = minAge;
        }

        public int MinAge { get; }
    }
}