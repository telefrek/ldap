using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP.IO
{
    /// <summary>
    /// Class for handling the protocol level communications
    /// </summary>
    public class LDAPConnection : ILDAPConnection
    {
        TcpClient _conn;
        Stream _transport;
        Stream _raw;
        int _messageId = 0;

        bool _sslEnabled;


        /// <summary>
        /// Internal constructor used to establish streams
        /// </summary>
        /// <param name="sslEnabled"></param>
        internal LDAPConnection(bool sslEnabled)
        {
            _conn = new TcpClient();
            _sslEnabled = sslEnabled;
            _transport = null;
        }

        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                await _conn.ConnectAsync(host, port);
                if (_sslEnabled)
                {
                    _raw = _conn.GetStream();

                    // Try to send an ssl message
                    var ms = new MemoryStream();

                    await ProtocolEncoding.WriteAsync(ms, Interlocked.Increment(ref _messageId));
                    await ProtocolEncoding.WriteAsync(ms, 23, EncodingScope.APPLICATION);
                    await ProtocolEncoding.WriteAsync(ms, "1.3.6.1.4.1.1466.20037");

                    ms.Position = 0;

                    await ProtocolEncoding.WriteAsync(_raw, ms);

                    _transport = new SslStream(_raw);
                    await (_transport as SslStream).AuthenticateAsClientAsync(host);
                }
                else
                {
                    _raw = _conn.GetStream();
                    _transport = null;
                }
            }
            catch (Exception e)
            {
                throw new LDAPException("Failed to connect", e);
            }
        }

        public async Task CloseAsync()
        {
            // Check for outstanding requests

            // Finish the session
            var ms = new MemoryStream();

            await ProtocolEncoding.WriteAsync(ms, Interlocked.Increment(ref _messageId));
            await ProtocolEncoding.WriteAsync(ms, 2, EncodingScope.APPLICATION);
            await ProtocolEncoding.WriteNullAsync(ms);

            ms.Position = 0;

            await ProtocolEncoding.WriteAsync(_raw, ms);
        }

        public async Task<bool> TryLoginAsync(string user, string password)
        {
            var ms = new MemoryStream();

            await ProtocolEncoding.WriteAsync(ms, Interlocked.Increment(ref _messageId));
            await ProtocolEncoding.WriteAsync(ms, 0, EncodingScope.APPLICATION);
            
            var payload = new MemoryStream();

            await ProtocolEncoding.WriteAsync(payload, 3);
            await ProtocolEncoding.WriteAsync(payload, user);
            await ProtocolEncoding.WriteAsync(payload, 0);
            await ProtocolEncoding.WriteAsync(payload, password);

            payload.Position = 0;

            await ProtocolEncoding.WriteAsync(ms, payload);

            ms.Position = 0;
            await ProtocolEncoding.WriteAsync(_raw, ms);

            return true;
        }

        bool _isDisposed = false;

        void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                if (_transport != null)
                {
                    _transport.Flush();
                    _transport.Close();
                }

                if (_conn != null)
                    _conn.Close();

                _transport = null;
                _conn = null;

                // Notify GC to ignore
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }

        public void Dispose() => Dispose(true);

        ~LDAPConnection() => Dispose(false);
    }
}