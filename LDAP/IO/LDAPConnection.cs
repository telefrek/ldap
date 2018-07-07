using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP.IO
{
    /// <summary>
    /// Class for handling the protocol level communications
    /// </summary>
    internal class LDAPConnection : ILDAPConnection
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

                    var options = new SslClientAuthenticationOptions
                    {
                        TargetHost = host,
                        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                        ClientCertificates = null,
                        LocalCertificateSelectionCallback = null,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                        RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
                        {
                            // Accept all...bad idea
                            return true;
                        },
                        ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http11 },
                        EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                    };

                    _transport = new SslStream(_raw);
                    await (_transport as SslStream).AuthenticateAsClientAsync(options, CancellationToken.None);
                    Reader = new LDAPReader(_transport);
                    Writer = new LDAPWriter(_transport);
                }
                else
                {
                    _raw = _conn.GetStream();
                    _transport = null;
                    Reader = new LDAPReader(_raw);
                    Writer = new LDAPWriter(_raw);
                }
            }
            catch (Exception e)
            {
                throw new LDAPException("Failed to connect", e);
            }
        }

        public async Task CloseAsync() => await TryQueueOperation(new UnbindRequest());

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
                else if (_raw != null)
                {
                    _raw.Flush();
                    _raw.Close();
                }

                if (_conn != null)
                    _conn.Close();

                _transport = null;
                _raw = null;
                _conn = null;

                // Notify GC to ignore
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }

        public void Dispose() => Dispose(true);

        public async Task<bool> TryQueueOperation(ProtocolOperation op)
        {
            await op.WriteAsync(Writer);
            return true;
        }

        public LDAPReader Reader { get; private set; }
        public LDAPWriter Writer { get; private set; }

        ~LDAPConnection() => Dispose(false);
    }
}