using System;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal abstract class LDAPRequest
    {
        public int MessageId { get; set; }

        public virtual bool HasResponse { get { return true; } }
        public virtual bool IsTerminating { get { return true; } }

        public abstract ProtocolOp Operation { get; }

        public async Task WriteAsync(LDAPWriter writer)
        {
            // Buffer the sequence to a memory stream first
            var opWriter = new LDAPWriter();
            await opWriter.WriteAsync(MessageId);

            // Write the op choice
            await WriteContentsAsync(opWriter);

            // Write the message as a generic sequence
            await writer.WriteAsync(opWriter);
        }

        protected abstract Task WriteContentsAsync(LDAPWriter writer);
    }
}