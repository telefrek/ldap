using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.IO
{
    internal sealed class BoundedStream : Stream
    {
        Stream _source;
        long _length;
        long _pos = 0;

        /// <summary>
        /// Defualt constructor for creating a bounded stream
        /// </summary>
        /// <param name="source">The wrapped stream</param>
        /// <param name="length">The length we can proccess from it</param>
        public BoundedStream(Stream source, long length)
        {
            _source = source;
            _length = length;
        }

        public override bool CanRead => _source.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => _source.CanWrite;

        public override long Length => _length;

        public override long Position
        {
            get => _pos;
            set => throw new InvalidOperationException("Position setting not supported");
        }

        public override int ReadByte()
        {
            if (_pos == _length)
                return -1;

            var n = _source.ReadByte();
            if (n >= 0)
                _pos++;

            return n;
        }

        public override void Flush() => _source.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var rem = _length - _pos;
            var n = rem > count ? _source.Read(buffer, offset, count)
                : _source.Read(buffer, offset, (int)rem);

            if (n >= 0)
                _pos += n;

            return n;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            var rem = _length - _pos;
            var n = rem > count ? await _source.ReadAsync(buffer, offset, count, token)
                : await _source.ReadAsync(buffer, offset, (int)rem, token);

            if (n >= 0)
                _pos += n;

            return n;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException("Seek not supported");

        public override void SetLength(long value) => throw new InvalidOperationException("Length cannot be changed");

        public override void Write(byte[] buffer, int offset, int count)
        {
            var rem = _length - _pos;
            if (count > rem) throw new IndexOutOfRangeException("Writing past end of stream not allowed");

            _source.Write(buffer, offset, count);
            _pos += count;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            var rem = _length - _pos;
            if (count > rem) throw new IndexOutOfRangeException("Writing past end of stream not allowed");

            await _source.WriteAsync(buffer, offset, count, token);
            _pos += count;
        }
    }
}