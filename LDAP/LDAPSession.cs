using System;
using System.Threading.Tasks;
using Telefrek.Security.LDAP.IO;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP
{
    public sealed class LDAPSession : IDisposable
    {
        ILDAPConnection _connection;
        LDAPOptions _options;

        public LDAPSession(LDAPOptions options)
        {
            _options = options;
            _connection = new LDAPConnection(options.IsSecured);
        }

        public async Task OpenAsync() => await _connection.ConnectAsync(_options.Host, _options.Port);

        public async Task<bool> TryLoginAsync(string user, string password)
        {
            var op = new BindRequest { Name = user, Authentication = new SimpleAuthentication { Credentials = password } };
            return await _connection.TryQueueOperation(op);
        }

        public async Task Close()
        {
            await _connection.CloseAsync();
            Dispose();
        }

        bool _isDisposed = false;

        void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                // Clear resources
                using (_connection) ;

                // Notify GC to ignore
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }

        public void Dispose() => Dispose(true);

        ~LDAPSession() => Dispose(false);
    }
}