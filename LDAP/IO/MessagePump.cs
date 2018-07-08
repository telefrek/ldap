using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP.IO
{
    /// <summary>
    /// Class is uesd to read messages from a stream and notify anyone who is interested in the events
    /// </summary>
    internal class MessagePump : IDisposable
    {
        Task _pumpThread;
        LDAPReader _reader;
        NetworkStream _raw;

        ConcurrentDictionary<int, TaskCompletionSource<ProtocolOperation>> _completions =
            new ConcurrentDictionary<int, TaskCompletionSource<ProtocolOperation>>();

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

        public Task<ProtocolOperation> GetResponse(int messageId, CancellationToken token = default(CancellationToken))
        {
            // Create the source and register the cancellation token if supplied
            var tcs = new TaskCompletionSource<ProtocolOperation>(TaskCreationOptions.AttachedToParent | TaskCreationOptions.RunContinuationsAsynchronously);
            if (token != default(CancellationToken))
                token.Register(() =>
                {
                    if (!tcs.Task.IsCompleted && !tcs.Task.IsCanceled && !tcs.Task.IsFaulted)
                    {
                        // Cancel the current task and remove from the completions
                        tcs.TrySetCanceled();
                        _completions.TryRemove(messageId, out tcs);
                    }
                });

            // register the callback
            _completions.AddOrUpdate(messageId, tcs, (msgId, old) => tcs);

            // Return the task hook
            return tcs.Task;
        }

        /// <summary>
        /// Internal method to watch the reader and notify when new messages are available
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
                            var message = await ProtocolOperation.ReadAsync(_reader);
                            
                            // Clear the task
                            TaskCompletionSource<ProtocolOperation> tcs;                            
                            if(_completions.TryRemove(message.MessageId, out tcs))
                                tcs.TrySetResult(message);
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

        public void Dispose() { }

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