using System.Linq;
using Authorization.Samples.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Samples.Controllers
{
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AppJwt)]
    public class JwtController : Controller
    {
        [HttpGet("jwt_hello")]
        public string AdminHello()
        {
            return $"Hello, {User.Claims.Single(c => c.Type == "name").Value}!";
        }

        [HttpGet("jwt_one_time")]
        [Authorize(Policies.RequireOneTimeJwtToken)]
        public string CheckOneTime()
        {
            var uid = User.Claims.First(c => c.Type == "uid").Value;

            return $"Successfully verified token with uid {uid}.";
        }
    }
}