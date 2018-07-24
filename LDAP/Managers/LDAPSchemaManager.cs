using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Default implementation of the ILDAPSchemaManager
    /// </summary>
    public class LDAPSchemaManager : ILDAPSchemaManager
    {
        ILDAPSession _session;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="session">The session to use for the manager</param>
        public LDAPSchemaManager(ILDAPSession session)
        {
            _session = session;
        }

        /// <summary>
        /// Gets the list of groups available in LDAP
        /// </summary>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>A collection of groups</returns>
        public async Task<List<string>> ListGroupsAsync(CancellationToken token)
        {
            var groups = new List<string>();
            var filter = new LDAPFilter { Description = "objectclass", FilterType = LDAPFilterType.Present };
            var res = await _session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                foreach (var obj in res.Objects)
                    groups.Add(obj.DistinguishedName);
            }

            return groups;
        }

        public async Task<bool> TryAuthenticateAsync(string name, string domain, string credentials, CancellationToken token)
        {
            
            return false;
        }
    }
}