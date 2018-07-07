using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal abstract class ProtocolOperation
    {
        static int _globalMessgeId = 0;

        public int MessageId { get; set; } = Interlocked.Increment(ref _globalMessgeId);

        public abstract ProtocolOp Operation { get; }

        public async Task WriteAsync(Stream target)
        {
            // Buffer the sequence to a memory stream first
            var ms = new MemoryStream();

            // Write the message id
            await ProtocolEncoding.WriteAsync(ms, MessageId);

            // Write the op choice
            await WriteContentsAsync(ms);

            // Move the stream back to the beginning for writes
            ms.Seek(0, SeekOrigin.Begin);
            await ProtocolEncoding.WriteAsync(target, ms, EncodingType.SEQUENCE, EncodingScope.UNIVERSAL);
        }

        protected virtual Task WriteContentsAsync(Stream target) => Task.CompletedTask;
    }
}