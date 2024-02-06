namespace Authorization.Samples.Authentication
{
    public static class Policies
    {
        public const string RequireTelephoneNumber = "RequireTelephoneNumber";
        public const string RequireGlobalAdminRealm = "RequireGlobalAdminRealm";
        public const string RequireAge18Plus = "RequireAge18+";
        public const string RequireOneTimeJwtToken = "RequireOneTimeJwtToken";
    }
}