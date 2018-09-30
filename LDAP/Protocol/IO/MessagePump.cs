using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        volatile MessagePumpNode _head;
        volatile MessagePumpNode _tail;
        AutoResetEvent _event;
        long _size;
        ILogger _log;

        volatile bool _isClosed = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reader">The reader to pump messages from</param>
        /// <param name="raw">The raw stream backing the reader</param>
        /// <param name="log"></param>
        public MessagePump(LDAPReader reader, NetworkStream raw, ILogger log)
        {
            _reader = reader;
            _raw = raw;
            _head = new MessagePumpNode();
            _tail = null;
            _event = new AutoResetEvent(false);
            _log = log;
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

        public IEnumerable<LDAPResponse> GetResponse(int messageId, CancellationToken token) =>
            // Create the source and register the cancellation token if supplied
            new MessagePumpEnumerator(_head, _event, messageId, token);

        /// <summary>
        /// public method to watch the reader and notify when new messages are available
        /// </summary>
        async Task Pump()
        {
            _log.LogInformation("LDAP pump started");
            // Need something to track if we should abandon existing tasks...etc.

            while (!_isClosed)
            {
                //if (_raw.DataAvailable)
                    try
                    {
                        // Ensure we can safely read the contents
                        if (await _reader.ReadAsync())
                        {
                            // read the next operation available
                            var message = await _reader.ReadResponseAsync();

                            if (message != null)
                            {
                                _log.LogInformation("Message received {0}", message.Operation);
                                if (_size == 0) // new
                                {
                                    _head.Message = message;
                                    _tail = _head;
                                    _size = 1;
                                }
                                else if (_size >= 16) // shouldn't hardcode the 16
                                {
                                    _head = _head.Next;
                                    _tail.Next = new MessagePumpNode { Message = message, Next = null };
                                    _tail = _tail.Next;
                                }
                                else
                                {
                                    _tail.Next = new MessagePumpNode { Message = message, Next = null };
                                    _tail = _tail.Next;
                                    _size++;
                                }

                                _event.Set();
                            }
                        }
                    }
                    catch (LDAPException ldapEx)
                    {
                        _log.LogError(ldapEx.ToString());
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex.ToString());
                    }
                // else 
                // {
                //     await Task.Delay(250);
                // }
            }
        }

        public void Dispose() => Dispose(true);

        bool _isDisposed = false;

        void Dispose(bool isDisposing)
        {
            _isClosed = true;
            _isDisposed = true;
        }
    }

    internal class MessagePumpNode
    {
        public LDAPResponse Message { get; set; }
        public MessagePumpNode Next { get; set; }
    }

    internal class MessagePumpEnumerator : IEnumerable<LDAPResponse>, IEnumerator<LDAPResponse>
    {
        EventWaitHandle _event;
        MessagePumpNode _node;
        CancellationToken _token;
        int _messageId;

        public MessagePumpEnumerator(MessagePumpNode node, EventWaitHandle evt, int messageId, CancellationToken token)
        {
            _node = node;
            _event = evt;
            _messageId = messageId;
            _token = token;
        }

        public LDAPResponse Current { get; set; } = null;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public IEnumerator<LDAPResponse> GetEnumerator() => this;

        public bool MoveNext()
        {
            // No messages yet, keep reading until we find one
            if (Current != null && Current.IsTerminating)
                return false;

            // Keep going until we find another message
            while (true)
            {
                if(_token.IsCancellationRequested)
                    return false;

                if (_node.Message == null)
                    _event.WaitOne(1000);

                if (_node.Message != Current && _node.Message.MessageId == _messageId)
                {
                    Current = _node.Message;
                    return true;
                }

                if (_node.Next != null)
                    _node = _node.Next;
                else
                    _event.WaitOne(1000);
            }
        }

        public void Reset() => throw new InvalidOperationException();

        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}