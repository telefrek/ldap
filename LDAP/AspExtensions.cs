using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Extendions for easy integration with aspnet core
    /// </summary>
    public static class AspExtensions
    {
        /// <summary>
        /// Add LDAP Authentication into the pipeline, still requires enabling in user auth
        /// </summary>
        /// <param name="services">The current service collection</param>
        /// <param name="config">The current configuration</param>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddLDAPAuth(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<LDAPConfiguration>(config.GetSection("ldap"));
            services.AddScoped<ILDAPSession, LDAPSession>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            return services;
        }

        /// <summary>
        /// Adds LDAP Authentication to the builder
        /// </summary>
        /// <param name="builder">The current application builder</param>
        /// <returns>The modified application builder</returns>
        public static IApplicationBuilder UseLDAPAuth(this IApplicationBuilder builder)
        {
            return builder;
        }
    }
}