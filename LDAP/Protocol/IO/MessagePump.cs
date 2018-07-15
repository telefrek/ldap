using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol;
using Telefrek.LDAP.Protocol.Collections;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol.IO
{
    /// <summary>
    /// Class is uesd to read messages from a stream and notify anyone who is interested in the events
    /// </summary>
    internal class MessagePump : IDisposable
    {
        Task _pumpThread;
        LDAPReader _reader;
        NetworkStream _raw;

        ConcurrentDictionary<int, StreamingEnumerator<LDAPResponse>> _completions =
            new ConcurrentDictionary<int, StreamingEnumerator<LDAPResponse>>();

        volatile bool _isClosed = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reader">The reader to pump messages from</param>
        /// <param name="raw">The raw stream backing the reader</param>
        public MessagePump(LDAPReader reader, NetworkStream raw)
        {
            _reader = reader;
            _raw = raw;
        }

        /// <summary>
        /// Starts the message pump
        /// </summary>
        public void Start()
        {
            if (_pumpThread == null)
                lock (this)
                {
                    if (_pumpThread == null)
                        _pumpThread = Task.Factory.StartNew(async () => await Pump()).Unwrap();
                }
        }

        /// <summary>
        /// Stop the message pump and wait for the task to complete
        /// </summary>
        public async Task StopAsync()
        {
            _isClosed = true;
            await _pumpThread;
        }

        public IEnumerable<LDAPResponse> GetResponse(int messageId)
        {
            // Create the source and register the cancellation token if supplied
            var e = new StreamingEnumerator<LDAPResponse>();

            // register the callback
            _completions.AddOrUpdate(messageId, e, (msgId, old) =>
            {
                old.Close();
                return e;
            });

            // Return the enumeration
            return e;
        }

        /// <summary>
        /// public method to watch the reader and notify when new messages are available
        /// </summary>
        async Task Pump()
        {
            // Need something to track if we should abandon existing tasks...etc.
            while (!_isClosed)
            {
                if (_raw.DataAvailable)
                    try
                    {
                        // Ensure we can safely read the contents
                        if (await _reader.ReadAsync())
                        {
                            // read the next operation available
                            var message = await _reader.ReadResponseAsync();

                            // Send this message along
                            StreamingEnumerator<LDAPResponse> e;
                            if (_completions.TryGetValue(message.MessageId, out e))
                                e.Add(message);

                            // Close the stream, not more objects are coming
                            if (message.IsTerminating && _completions.TryRemove(message.MessageId, out e))
                                e.Close();
                        }
                    }
                    catch (LDAPException)
                    {
                        // TODO: Handle
                    }
                    catch (Exception)
                    {
                        // This is bad...
                    }
                else await Task.Delay(250);
            }
        }

        public void Dispose() => Dispose(true);

        bool _isDisposed = false;

        void Dispose(bool isDisposing)
        {
            if (isDisposing && !_isDisposed)
            {

            }

            _isDisposed = true;
        }
    }
}