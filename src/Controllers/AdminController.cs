using Authorization.Samples.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Samples.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : Controller
    {
        [HttpGet("admin_hello")]
        [Authorize(Policies.RequireGlobalAdminRealm)]
        public string AdminHello()
        {
            return $"Hello, {User.Identity!.Name}!";
        }
        
        
        [HttpGet("hello_18+")]
        [Authorize(Policies.RequireAge18Plus)]
        public string Hello18Plus()
        {
            return $"Hello, {User.Identity!.Name}!";
        }
        
        [HttpGet("anonymous_hello")]
        [AllowAnonymous]
        public string AnonymousHello()
        {
            return "Hello, anonymous!";
        }
    }
}