using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telefrek.Security.LDAP.IO;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP
{
    /// <summary>
    /// Represents an LDAP session and is the main entrypoint for the library
    /// </summary>
    public sealed class LDAPSession : ILDAPSession
    {
        ILDAPConnection _connection;
        LDAPConfiguration _options;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Options for LDAP communication</param>
        public LDAPSession(IOptions<LDAPConfiguration> options)
        {
            _options = options.Value;
            _connection = new LDAPConnection(_options.IsSecured);
        }

        /// <summary>
        /// Starts the session asynchronously
        /// </summary>
        public async Task StartAsync() => await _connection.ConnectAsync(_options.Host, _options.Port);

        /// <summary>
        /// Attempts a login with the given credentials
        /// </summary>
        /// <param name="domainUser">The domain qualified user</param>
        /// <param name="credentials">The associated credentials</param>
        /// <param name="token"></param>
        /// <returns>True if the login was successful</returns>
        public async Task<bool> TryLoginAsync(string domainUser, string credentials, CancellationToken token)
        {
            var op = new BindRequest { Name = domainUser, Authentication = new SimpleAuthentication { Credentials = credentials } };
            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                var res = msg as LDAPResponse;
                if (res != null) return res.ResultCode == 0;
            }

            return false;
        }

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="scope"></param>
        /// <param name="aliasing"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, CancellationToken token)
        {
            var op = new SearchRequest { ObjectDN = dn, Scope = scope, Aliasing = aliasing };
            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                var res = msg as LDAPResponse;
                if (res != null) return res.ResultCode == 0;
            }

            return false;
        }

        /// <summary>
        /// Ends the session and closes all resources asynchronously
        /// </summary>
        /// <remarks>
        /// Note that any outstanding requests will be abandoned and may not finish appropriately
        /// and certain states will require this operation to wait for completion before stopping
        /// </remarks>
        public async Task CloseAsync()
        {
            await _connection.CloseAsync();
            Dispose();
        }

        /// <summary>
        /// Dispose of the object resources
        /// </summary>
        public void Dispose() => Dispose(true);

        bool _isDisposed = false;

        void Dispose(bool disposing)
        {
            // Check state flags
            if (disposing && !_isDisposed)
            {
                // Ensure we cleanup connection resources
                if (_connection != null)
                    _connection.Dispose();

                _connection = null;

                // Notify GC to ignore
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Destructor for releasing resources via GC callbacks
        /// </summary>
        /// <returns></returns>
        ~LDAPSession() => Dispose(false);
    }
}