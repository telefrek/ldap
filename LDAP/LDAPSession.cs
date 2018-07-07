using System;
using System.Threading.Tasks;
using Telefrek.Security.LDAP.IO;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP
{
    /// <summary>
    /// Represents an LDAP session and is the main entrypoint for the library
    /// </summary>
    public sealed class LDAPSession : IDisposable
    {
        ILDAPConnection _connection;
        LDAPOptions _options;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Options for LDAP communication</param>
        public LDAPSession(LDAPOptions options)
        {
            _options = options;
            _connection = new LDAPConnection(options.IsSecured);
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
        /// <returns>True if the login was successful</returns>
        public async Task<bool> TryLoginAsync(string domainUser, string credentials)
        {
            var op = new BindRequest { Name = domainUser, Authentication = new SimpleAuthentication { Credentials = credentials } };
            return await _connection.TryQueueOperation(op);
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