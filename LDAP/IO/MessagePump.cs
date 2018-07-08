using System;
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

        volatile bool _isClosed = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reader">The reader to pump messages from</param>
        public MessagePump(LDAPReader reader)
        {
            _reader = reader;
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

        /// <summary>
        /// Internal method to watch the reader and notify when new messages are available
        /// </summary>
        async Task Pump()
        {
            while (!_reader.IsComplete && !_isClosed)
            {
                if (_reader.HasData)
                {
                    try
                    {
                        // Ensure we can safely read the contents
                        if (await _reader.ReadAsync())
                        {
                            // read the next operation available
                            var message = await ProtocolOperation.ReadAsync(_reader);

                            // This is costly and not threaded appropriatly, and relies on clients doing the right thing...not a good idea
                            if (MessageAvailable != null)
                                MessageAvailable.Invoke(this, new MessageAvailableEventArgs { MessageId = message.MessageId, Message = message });
                        }
                    }
                    catch (LDAPException)
                    {
                        // TODO: Handle
                    }
                    catch(Exception)
                    {
                        // This is bad...
                    }
                }
                else await Task.Delay(500);
            }
        }

        public delegate void MessageAvailableHandler(object sender, MessageAvailableEventArgs args);
        public event MessageAvailableHandler MessageAvailable;

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

    internal class MessageAvailableEventArgs : EventArgs
    {
        public int MessageId { get; set; }
        public ProtocolOperation Message { get; set; }
    }
}