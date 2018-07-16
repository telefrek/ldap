using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Default implementation of the IUserManager
    /// </summary>
    public class LDAPUserManager : ILDAPUserManager
    {
        /// <summary>
        /// Locates the user, if they exist and session has permissions
        /// </summary>
        /// <param name="name">The user name</param>
        /// <param name="domain">The user domain</param>
        /// <param name="session">The current session to search</param>
        /// <param name="token">The token to use for the call</param>
        /// <returns>A LDAPUser, if one exists for the given parameters</returns>
        public async Task<LDAPUser> FindUserAsync(string name, string domain, ILDAPSession session, CancellationToken token)
        {
            var dnBuilder = new StringBuilder();
            dnBuilder.AppendFormat("cn={0}", name);
            foreach (var dc in domain.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                dnBuilder.AppendFormat(",dc={0}", dc);

            // Not entirely sure cn is the best way to go about it, but works for now
            var filter = new LDAPFilter { Value = string.Format("cn={0}", name), FilterType = LDAPFilterType.Present };
            var res = await session.TrySearch(dnBuilder.ToString(), LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                foreach (var obj in res.Objects)
                    return new LDAPUser(obj);
            }

            return null;
        }
    }
}