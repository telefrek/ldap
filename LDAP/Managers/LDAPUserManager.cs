using System;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Default implementation of the IUserManager
    /// </summary>
    public class LDAPUserManager : ILDAPUserManager
    {
        ILDAPSession _session;
        LDAPManagerConfiguration _options;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="session">The session to use for the manager</param>
        public LDAPUserManager(IOptions<LDAPManagerConfiguration> options, ILDAPSession session)
        {
            _options = options.Value;
            _session = session;
        }

        /// <summary>
        /// Tries to authenticate the given credentials and generate a claims principal to use
        /// </summary>
        /// <param name="name">The username</param>
        /// <param name="domain">The user domain</param>
        /// <param name="credentials">The user credentials</param>
        /// <param name="token">The token to use for the call</param>
        /// <returns>A role based ClaimsPrincipal if valid credentials, else null</returns>
        public async Task<ClaimsPrincipal> TryAuthenticate(string name, string domain, string credentials, CancellationToken token)
        {
            await _session.StartAsync();

            if(await _session.TryBindAsync(name.AsDistinguishedName(domain), credentials, token))
            {
                // Create the identity from the user info
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, name));
                identity.AddClaim(new Claim(ClaimTypes.Name, name));

                // Return the initialized principal
                return new ClaimsPrincipal(identity);
            }

            return null;
        }

        /// <summary>
        /// Locates the user, if they exist and session has permissions
        /// </summary>
        /// <param name="name">The user name</param>
        /// <param name="domain">The user domain</param>
        /// <param name="token">The token to use for the call</param>
        /// <returns>A LDAPUser, if one exists for the given parameters</returns>
        public async Task<LDAPUser> FindUserAsync(string name, string domain, CancellationToken token)
        {
            // Ensure the session is started
            await _session.StartAsync();

            // Not entirely sure cn is the best way to go about it, but works for now
            var filter = new LDAPFilter { Description = "cn", Value = name, FilterType = LDAPFilterType.EqualityMatch };
            var res = await _session.TrySearch(name.AsDistinguishedName(domain), LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                foreach (var obj in res.Objects)
                    return new LDAPUser(obj);
            }

            return null;
        }
    }
}