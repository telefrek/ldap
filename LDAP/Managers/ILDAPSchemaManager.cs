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
        Task<List<LDAPObject>> ListGroupsAsync(CancellationToken token);

        /// <summary>
        /// Creates a new group with the given name
        /// </summary>
        /// <param name="groupName">The new group name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>The new object if it was created successfully</returns>
        Task<LDAPObject> CreateGroupAsync(string groupName, CancellationToken token);

        /// <summary>
        /// Deletes the group with the given name
        /// </summary>
        /// <param name="groupName">The group name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>Flag to indicate if the operation was successful</returns>
        Task<bool> TryDeleteGroupAsync(string groupName, CancellationToken token);

        /// <summary>
        /// Gets the list of roles available in LDAP
        /// </summary>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>A collection of roles</returns>
        Task<List<LDAPObject>> ListRolesAsync(CancellationToken token);

        /// <summary>
        /// Creates a new role with the given name
        /// </summary>
        /// <param name="roleName">The new role name</param>
        /// <param name="ownerDN">The group owner distinguished name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>The new object if it was created successfully</returns>
        Task<LDAPObject> CreateRoleAsync(string roleName, string ownerDN, CancellationToken token);

        /// <summary>
        /// Deletes the role with the given name
        /// </summary>
        /// <param name="roleName">The role name</param>
        /// <param name="token">The cancellation token for the operation</param>
        /// <returns>Flag to indicate if the operation was successful</returns>
        Task<bool> TryDeleteRoleAsync(string roleName, CancellationToken token);
    }
}