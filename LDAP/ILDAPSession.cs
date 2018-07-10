using System;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP
{
    public interface ILDAPSession : IDisposable
    {

        /// <summary>
        /// Starts the session asynchronously
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Attempts a login with the given credentials
        /// </summary>
        /// <param name="domainUser">The domain qualified user</param>
        /// <param name="credentials">The associated credentials</param>
        /// <param name="token"></param>
        /// <returns>True if the login was successful</returns>
        Task<bool> TryLoginAsync(string domainUser, string credentials, CancellationToken token);

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="scope"></param>
        /// <param name="aliasing"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, CancellationToken token);

        /// <summary>
        /// Ends the session and closes all resources asynchronously
        /// </summary>
        /// <remarks>
        /// Note that any outstanding requests will be abandoned and may not finish appropriately
        /// and certain states will require this operation to wait for completion before stopping
        /// </remarks>
        Task CloseAsync();

    }
}