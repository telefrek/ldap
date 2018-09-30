using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Manages higher level user objects and does the translation to/from LDAP
    /// </summary>
    public interface ILDAPUserManager : IDisposable
    {
        /// <summary>
        /// Locates the user, if they exist and session has permissions
        /// </summary>
        /// <param name="name">The user name</param>
        /// <param name="domain">The user domain</param>
        /// <param name="token">The token to use for the call</param>
        /// <returns>A LDAPUser, if one exists for the given parameters</returns>
        Task<LDAPUser> FindUserAsync(string name, string domain, CancellationToken token);

        /// <summary>
        /// Tries to authenticate the given credentials and generate a claims principal to use
        /// </summary>
        /// <param name="name">The username</param>
        /// <param name="domain">The user domain</param>
        /// <param name="credentials">The user credentials</param>
        /// <param name="token">The token to use for the call</param>
        /// <returns>A role based ClaimsPrincipal if valid credentials, else null</returns>
        Task<ClaimsPrincipal> TryAuthenticate(string name, string domain, string credentials, CancellationToken token);
    }
}