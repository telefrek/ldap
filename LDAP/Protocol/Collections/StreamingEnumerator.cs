using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Telefrek.LDAP.Protocol.Collections
{
    /// <summary>
    /// Internal class for handling streaming enumerations in a pub/sub model
    /// across threads without the TPL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class StreamingEnumerator<T> : IEnumerable<T>, IEnumerator<T>
    {
        object _syncLock = new object();

        Queue<T> _buffer = new Queue<T>();
        volatile bool _isClosed = false;

        public void Add(T obj)
        {
            lock (_syncLock)
            {
                // Add and notify
                _buffer.Enqueue(obj);
                Monitor.PulseAll(_syncLock);
            }
        }

        /// <summary>
        /// Closes the enumeration
        /// </summary>
        public void Close()
        {
            lock (_syncLock)
            {
                _isClosed = true;
                Monitor.PulseAll(_syncLock);
            }
        }

        public T Current
        {
            get
            {
                lock (_syncLock)
                    return _buffer.Dequeue();
            }
        }

        object IEnumerator.Current
        {
            get
            {
                lock (_syncLock)
                    return _buffer.Dequeue();
            }
        }

        public void Dispose() => Close();

        public IEnumerator<T> GetEnumerator() => this;

        /// <summary>
        /// Blocks the thread until another object is available or the stream is ended
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            lock (_syncLock)
            {
                // Check if we already have data prepped
                if (_buffer.Count > 0)
                    return true;

                // Keep waiting until closed
                while (!_isClosed)
                {
                    // wait for a new notification
                    if (Monitor.Wait(_syncLock, 1000))
                    {
                        // Check the buffer size
                        if (_buffer.Count > 0)
                            return true;
                    }
                }

                // Only let the current buffer count through (may have had something added between)
                return _buffer.Count > 0;
            }
        }

        public void Reset() => throw new InvalidOperationException("Cannot reset streaming enumeration");

        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}