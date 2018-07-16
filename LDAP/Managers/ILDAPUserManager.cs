using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Manages higher level user objects and does the translation to/from LDAP
    /// </summary>
    public interface ILDAPUserManager
    {
        /// <summary>
        /// Locates the user, if they exist and session has permissions
        /// </summary>
        /// <param name="name">The user name</param>
        /// <param name="domain">The user domain</param>
        /// <param name="session">The current session to search</param>
        /// <param name="token">The token to use for the call</param>
        /// <returns>A LDAPUser, if one exists for the given parameters</returns>
        Task<LDAPUser> FindUserAsync(string name, string domain, ILDAPSession session, CancellationToken token);
    }
}