using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Manager for LDAP schema data
    /// </summary>
    public interface ILDAPSchemaManager
    {
        /// <summary>
        /// Gets the list of groups available in LDAP
        /// </summary>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>A collection of groups</returns>
        Task<List<string>> ListGroupsAsync(CancellationToken token);
    }
}