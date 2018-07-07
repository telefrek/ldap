using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    /// <summary>
    /// Default protocol operation class used to pass messages back and forth
    /// </summary>
    internal abstract class ProtocolOperation
    {
        static int _globalMessgeId = 0;

        public int MessageId { get; set; } = Interlocked.Increment(ref _globalMessgeId);

        public abstract ProtocolOp Operation { get; }

        public async Task WriteAsync(LDAPWriter writer)
        {
            // Buffer the sequence to a memory stream first
            var opWriter = new LDAPWriter(new MemoryStream());
            await opWriter.WriteAsync(MessageId);

            // Write the op choice
            await WriteContentsAsync(opWriter);
            
            // Write the message as a generic sequence
            await writer.WriteAsync(opWriter);
        }

        protected abstract Task WriteContentsAsync(LDAPWriter target);
    }
}