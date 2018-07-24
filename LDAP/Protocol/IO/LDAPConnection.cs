using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol.IO
{
    /// <summary>
    /// Class for handling the protocol level communications
    /// </summary>
    internal class LDAPConnection : ILDAPConnection
    {
        int _globalMessgeId = 0;
        TcpClient _conn;
        SslStream _transport;
        NetworkStream _raw;
        MessagePump _pump;
        LDAPConnectionState _state;
        int _messageId = 0;
        bool _sslEnabled;

        /// <summary>
        /// public constructor used to establish streams
        /// </summary>
        /// <param name="sslEnabled"></param>
        public LDAPConnection(bool sslEnabled)
        {
            _conn = new TcpClient();
            _sslEnabled = sslEnabled;
            _transport = null;
            _state = LDAPConnectionState.NotInitialized;
        }

        public LDAPConnectionState State => _state;

        /// <summary>
        /// Connect to the given host on the port asynchronously
        /// </summary>
        /// <param name="host">The host to connect to</param>
        /// <param name="port">The port to use for communication</param>
        public async Task ConnectAsync(string host, int port)
        {
            // Don't reconnect
            if(_state == LDAPConnectionState.Connected)
                return;

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

                // Create the pump and start it
                _pump = new MessagePump(Reader, _raw);
                _pump.Start();
                _state = LDAPConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = LDAPConnectionState.Faulted;
                throw new LDAPException("Failed to connect", e);
            }
        }

        public async Task CloseAsync()
        {
            // Should probably throw if this is not the case
            if(_state != LDAPConnectionState.Connected)
                return;

            await TryQueueOperation(new UnbindRequest(), CancellationToken.None);
            if (_pump != null)
            {
                await _pump.StopAsync();
                _pump.Dispose();
            }
            _state = LDAPConnectionState.Closed;
        }

        public async Task<IEnumerable<LDAPResponse>> TryQueueOperation(LDAPRequest request, CancellationToken token)
        {
            request.MessageId = Interlocked.Increment(ref _globalMessgeId);

            try
            {
                if (request.HasResponse)
                {
                    var response = _pump.GetResponse(request.MessageId);
                    await request.WriteAsync(Writer);

                    return response;
                }
                else await request.WriteAsync(Writer);
            }
            catch (AggregateException)
            {
            }

            return new LDAPResponse[] { };
        }

        /// <summary>
        /// Gets the connection reader
        /// </summary>
        public LDAPReader Reader { get; private set; }

        /// <summary>
        /// Gets the connection writer
        /// </summary>
        public LDAPWriter Writer { get; private set; }

        /// <summary>
        /// Dispose of all connection resources
        /// </summary>
        public void Dispose() => Dispose(true);

        bool _isDisposed = false;

        void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                if (_pump != null)
                    _pump.Dispose();

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
                _pump = null;

                // Notify GC to ignore
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }

        ~LDAPConnection() => Dispose(false);
    }
}