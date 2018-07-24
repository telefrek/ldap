using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telefrek.LDAP.Managers;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Extendions for easy integration with aspnet core
    /// </summary>
    public static class LDAPExtensions
    {
        /// <summary>
        /// Formats a string in place
        /// </summary>
        /// <param name="format">The format string to use</param>
        /// <param name="data">The parameters to use for the format string</param>
        /// <returns>A formatted string</returns>
        public static string ToFormat(this string format, params object[] data) => string.Format(format, data);

        /// <summary>
        /// Transform a username and domain into a Distinguished name
        /// </summary>
        /// <param name="username">The user name</param>
        /// <param name="domain">The target domain</param>
        /// <returns>A LDAP distinguished name</returns>
        public static string AsDistinguishedName(this string username, string domain)
        {
            var dnBuilder = new StringBuilder();
            dnBuilder.AppendFormat("cn={0}", username);
            foreach (var dc in domain.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                dnBuilder.AppendFormat(",dc={0}", dc);

            return dnBuilder.ToString();
        }

        /// <summary>
        /// Add LDAP Authentication into the pipeline, still requires enabling in user auth
        /// </summary>
        /// <param name="services">The current service collection</param>
        /// <param name="config">The current configuration</param>
        /// <returns>The modified service collection</returns>
        public static IServiceCollection AddLDAPAuth(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<LDAPConfiguration>(config.GetSection("ldap"));
            services.Configure<LDAPManagerConfiguration>(config.GetSection("ldap_mgmt"));
            services.AddScoped<ILDAPSession, LDAPSession>();
            services.AddScoped<ILDAPUserManager, LDAPUserManager>();
            services.AddScoped<ILDAPSchemaManager, LDAPSchemaManager>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.LoginPath = "/Login";
                   options.Cookie.HttpOnly = false;
                   options.Cookie.Path = "/";
                   options.Cookie.SecurePolicy = CookieSecurePolicy.None;
               });

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