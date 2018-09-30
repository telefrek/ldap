using System.Collections.Generic;
using System.Linq;
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
        public async Task<List<LDAPObject>> ListGroupsAsync(CancellationToken token)
        {
            var groups = new List<LDAPObject>();
            var filter = new LDAPFilter { Description = "objectClass", Value = "organizationalUnit", FilterType = LDAPFilterType.EqualityMatch };
            var res = await _session.TrySearch(_session.CurrentScope.Domain, LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                foreach (var obj in res.Objects)
                    groups.Add(obj);
            }

            return groups;
        }

        /// <summary>
        /// Creates a new group with the given name
        /// </summary>
        /// <param name="groupName">The new group name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>The new object if it was created successfully</returns>
        public async Task<LDAPObject> CreateGroupAsync(string groupName, CancellationToken token)
        {
            try
            {
                var newRole = new LDAPObject
                {
                    DistinguishedName = "ou=" + groupName + "," + _session.CurrentScope.Domain,
                    Domain = _session.CurrentScope.Domain,
                    Attributes = new List<LDAPAttribute>()
                };

                newRole.Attributes.Add(new LDAPAttribute { Description = "ou", Values = new List<string>() { groupName } });
                newRole.Attributes.Add(new LDAPAttribute { Description = "objectClass", Values = new List<string>() { "organizationalUnit" } });

                var res = await _session.TryAdd(newRole, token);
                if (res != null && res.WasSuccessful)
                    return res.Objects.FirstOrDefault();
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Deletes the group with the given name
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>Flag to indicate if the operation was successful</returns>
        public async Task<bool> TryDeleteGroupAsync(string groupName, CancellationToken token)
        {
            var roles = new List<LDAPObject>();
            var filter = new LDAPFilter { Description = "objectClass", Value = "organizationalUnit", FilterType = LDAPFilterType.EqualityMatch };
            filter &= new LDAPFilter { Description = "ou", Value = groupName, FilterType = LDAPFilterType.EqualityMatch };
            var res = await _session.TrySearch(_session.CurrentScope.Domain, LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                var success = true;
                foreach (var obj in res.Objects)
                {
                    res = await _session.TryRemove(obj, token);
                    success &= res.WasSuccessful;

                    // Abort early
                    if(!success)
                        return success;
                }

                return success;
            }

            return false;
        }

        /// <summary>
        /// Gets the list of groups available in LDAP
        /// </summary>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>A collection of groups</returns>
        public async Task<List<LDAPObject>> ListRolesAsync(CancellationToken token)
        {
            var roles = new List<LDAPObject>();
            var filter = new LDAPFilter { Description = "objectClass", Value = "groupOfNames", FilterType = LDAPFilterType.EqualityMatch };
            var res = await _session.TrySearch(_session.CurrentScope.Domain, LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                foreach (var obj in res.Objects)
                    roles.Add(obj);
            }

            return roles;
        }

        /// <summary>
        /// Creates a new role with the given name
        /// </summary>
        /// <param name="roleName">The new role name</param>
        /// <param name="ownerDN">The group owner distinguished name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>The new object if it was created successfully</returns>
        public async Task<LDAPObject> CreateRoleAsync(string roleName, string ownerDN, CancellationToken token)
        {
            try
            {
                var newRole = new LDAPObject
                {
                    DistinguishedName = "cn=" + roleName + "," + _session.CurrentScope.Domain,
                    Domain = _session.CurrentScope.Domain,
                    Attributes = new List<LDAPAttribute>()
                };

                newRole.Attributes.Add(new LDAPAttribute { Description = "cn", Values = new List<string>() { roleName } });
                newRole.Attributes.Add(new LDAPAttribute { Description = "member", Values = new List<string>() { ownerDN } });
                newRole.Attributes.Add(new LDAPAttribute { Description = "objectClass", Values = new List<string>() { "groupOfNames" } });

                var res = await _session.TryAdd(newRole, token);
                if (res != null && res.WasSuccessful)
                    return res.Objects.FirstOrDefault();
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Deletes the role with the given name
        /// </summary>
        /// <param name="roleName">The new role name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>Flag to indicate if the operation was successful</returns>
        public async Task<bool> TryDeleteRoleAsync(string roleName, CancellationToken token)
        {
            var roles = new List<LDAPObject>();
            var filter = new LDAPFilter { Description = "objectClass", Value = "groupOfNames", FilterType = LDAPFilterType.EqualityMatch };
            filter &= new LDAPFilter { Description = "cn", Value = roleName, FilterType = LDAPFilterType.EqualityMatch };
            var res = await _session.TrySearch(_session.CurrentScope.Domain, LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, filter, token);

            if (res.WasSuccessful)
            {
                var success = true;
                foreach (var obj in res.Objects)
                {
                    res = await _session.TryRemove(obj, token);
                    success &= res.WasSuccessful;

                    // Abort early
                    if(!success)
                        return success;
                }

                return success;
            }

            return false;
        }
    }
}