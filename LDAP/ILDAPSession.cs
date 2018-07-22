using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Interface for manipulating an LDAP Session
    /// </summary>
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
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<LDAPResult> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, LDAPFilter filter, CancellationToken token);

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="scope"></param>
        /// <param name="aliasing"></param>
        /// <param name="filter"></param>
        /// <param name="attributes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<LDAPResult> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, LDAPFilter filter, string[] attributes, CancellationToken token);

        /// <summary>
        /// Try to add a record to the directory
        /// </summary>
        /// <param name="obj">The LDAP Entity to add</param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        Task<LDAPResult> TryAdd(LDAPObject obj, CancellationToken token);

        /// <summary>
        /// Try to add a record to the directory
        /// </summary>
        /// <param name="obj">The LDAP Entity to add</param>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        /// <param name="update"></param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        Task<LDAPResult> TryModify(LDAPObject obj, ICollection<LDAPAttribute> add, ICollection<LDAPAttribute> remove, ICollection<LDAPAttribute> update, CancellationToken token);

        /// <summary>
        /// Try to remove a record from the directory
        /// </summary>
        /// <param name="obj">The record to remove</param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        Task<LDAPResult> TryRemove(LDAPObject obj, CancellationToken token);

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