using Authorization.Samples.Authentication;
using Authorization.Samples.Authorization;
using Authorization.Samples.Authorization.AuthorizationHandlers;
using Authorization.Samples.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Threading;
using StackExchange.Redis;

namespace Authorization.Samples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AdditionalAuthenticationOptions>(Configuration.GetSection("AdditionalAuthenticationOptions"));
            services.Configure<JwtSchemeOptions>(Configuration.GetSection("Jwt"));

            services.AddAuthentication(AuthenticationSchemes.AppJwt)
                .AddCustomJwtToken();

            services.AddAuthorization(op =>
            {
                op.DefaultPolicy =
                    new AuthorizationPolicyBuilder(AuthenticationSchemes.AppJwt)
                        .RequireAuthenticatedUser()
                        .Build();
                op.AddPolicy(Policies.RequireTelephoneNumber,
                    new AuthorizationPolicyBuilder(AuthenticationSchemes.AppJwt)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new HasAllAdditionalAttributesRequirement())
                        .Build());
                op.AddPolicy(Policies.RequireGlobalAdminRealm,
                    new AuthorizationPolicyBuilder(AuthenticationSchemes.AppJwt)
                        .RequireAuthenticatedUser()
                        .RequireClaim(AppClaimTypes.AdminRealm, "global")
                        .Build());
                op.AddPolicy(Policies.RequireAge18Plus,
                    new AuthorizationPolicyBuilder(AuthenticationSchemes.AppJwt)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new MinAgeRequirement(18))
                        .Build());
                op.AddPolicy(Policies.RequireOneTimeJwtToken,
                    new AuthorizationPolicyBuilder(AuthenticationSchemes.AppJwt)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new OneTimeJwtTokenRequirement("token."))
                        .Build());
            });

            services.AddSingleton(_ => new AsyncLazy<IConnectionMultiplexer>(async () =>
                await ConnectionMultiplexer.ConnectAsync(Configuration["Redis:Configuration"])));
            services.AddSingleton(c => new AsyncLazy<IDatabaseAsync>(async () =>
            {
                var connection = await c.GetRequiredService<AsyncLazy<IConnectionMultiplexer>>().GetValueAsync();

                return connection.GetDatabase();
            }));

            services.AddTransient<IAuthorizationHandler, AgeAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, HasAllAdditionalAttributesAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, OneTimeJwtTokenAuthorizationHandler>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}