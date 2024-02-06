using System.Linq;
using Authorization.Samples.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Samples.Controllers
{
    public class SampleController : Controller
    {
        [HttpGet("sample")]
        [Authorize(Roles = Roles.AppUser)]
        public string Get()
        {
            var b = User.IsInRole("group2");
            return $"Hello, {User.Identity.Name}";
        }

        [HttpGet("telephone_number")]
        [Authorize(Policies.RequireTelephoneNumber)]
        public string ShowTelephoneNumber()
        {
            return $"Your telephone number is {User.Claims.Single(c => c.Type == "telephoneNumber").Value}";
        }
    }
}