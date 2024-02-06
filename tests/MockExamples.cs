using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Authorization.Samples;
using Authorization.Samples.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;
using AuthenticationSchemes = Authorization.Samples.Authentication.AuthenticationSchemes;

namespace Authorization.Tests
{
    public class MockExamples
    {
        private CancellationTokenSource _cancel;
        private IHost _host;
        private HttpClient _client;

        [SetUp]
        public async Task Setup()
        {
            var authenticationHandlerProvider = new Mock<IAuthenticationHandlerProvider>();

            TestAuthenticationHandler.ClearSetups();
            authenticationHandlerProvider.Setup(p => p.GetHandlerAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync((HttpContext context, string scheme) => new TestAuthenticationHandler(scheme, context));

            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>().UseTestServer())
                .ConfigureServices(services => services
                    .AddTransient(_ => authenticationHandlerProvider.Object)
                    .AddLogging(op => op.ClearProviders().AddNUnit()))
                .Build();
            _cancel = new CancellationTokenSource(100_000);

            await _host.StartAsync(_cancel.Token);

            var server = _host.Services.GetRequiredService<IServer>().ShouldBeOfType<TestServer>();
            _client = server.CreateClient();
        }

        [TearDown]
        public async Task TearDown()
        {
            _client.Dispose();
            await _host.StopAsync(_cancel.Token);
            _host.Dispose();
            _cancel.Dispose();
        }

        [Test]
        public async Task ShouldNotAllowAnonymous()
        {
            var res = await _client.GetAsync("sample");

            res.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Test]
        [TestCase("user1")]
        public async Task ShouldAuthorizeWithRole(string username)
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.AppJwt,
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, Roles.AppUser));

            var res = await _client.GetStringAsync("sample");

            res.ShouldBe($"Hello, {username}");
        }
        
        [Test]
        public async Task ShouldNotAuthorizeWithoutRole()
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.AppJwt,
                new Claim(ClaimTypes.Name, "user1"));

            var res = await _client.GetAsync("sample");

            res.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }
        
        [Test]
        [TestCase("2324321")]
        public async Task ShouldAuthorizeWithAdditionalClaim(string telephoneNumber)
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.AppJwt,
                new Claim(ClaimTypes.Name, "user1"),
                new Claim(AppClaimTypes.TelephoneNumber, telephoneNumber));

            var res = await _client.GetStringAsync("telephone_number");

            res.ShouldContain(telephoneNumber);
        }

        [Test]
        public async Task ShouldNotAuthorizeWithoutAdditionalClaim()
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.AppJwt,
                new Claim(ClaimTypes.Name, "user1"));

            var res = await _client.GetAsync("telephone_number");

            res.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        
        [Test]
        public async Task ShouldAuthorizeWithRequirement()
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.AppJwt,
                //new Claim(ClaimTypes.Name, "user1"),
                new Claim(ClaimTypes.Role, Roles.Admin),
                new Claim(AppClaimTypes.AdminRealm, "global"));

            var res = await _client.GetAsync("admin_hello");

            res.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        //--1
        [Test]
        public async Task ShouldAuthorizeWithAnotherScheme()
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.Social,
                new Claim(ClaimTypes.Name, "user1"));

            var res = await _client.GetAsync("social_hello");

            res.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        
        [Test]
        [TestCase(19, HttpStatusCode.OK)]
        [TestCase(10, HttpStatusCode.Forbidden)]
        public async Task ShouldAuthorizeWithAssertion(int age, HttpStatusCode code)
        {
            TestAuthenticationHandler.SetupSuccess(AuthenticationSchemes.AppJwt,
                new Claim(ClaimTypes.Name, "user1"),
                new Claim(AppClaimTypes.Age, age.ToString()),
                new Claim(ClaimTypes.Role, Roles.Admin));

            var res = await _client.GetAsync("hello_18+");

            res.StatusCode.ShouldBe(code);
        }
        
        [Test]
        public async Task ShouldAllowAnonymous()
        {
            var res = await _client.GetAsync("anonymous_hello");

            res.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}