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

        public virtual bool HasResponse { get { return true; } }

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

        public static async Task<ProtocolOperation> ReadAsync(LDAPReader reader)
        {
            if (reader.Scope != EncodingScope.UNIVERSAL || reader.Tag != (int)EncodingType.SEQUENCE)
                throw new LDAPProtocolException("Invalid envelope");

            var messageReader = reader.CreateReader();
            if (!await messageReader.ReadAsync())
                throw new LDAPProtocolException("Truncated envelope");

            // Read the message id
            var messageId = await messageReader.ReadAsIntAsync();

            if (!await messageReader.ReadAsync())
                throw new LDAPProtocolException("Truncated envelope");

            switch ((ProtocolOp)messageReader.Tag)
            {
                case ProtocolOp.BIND_RESPONSE:
                    var op = new BindResponse { MessageId = messageId };
                    await op.ReadContentsAsync(messageReader);

                    return op;
                default:
                    throw new LDAPProtocolException("Unknown/Invalid protocol operation");
            }
        }

        protected abstract Task WriteContentsAsync(LDAPWriter writer);

        protected abstract Task ReadContentsAsync(LDAPReader reader);
    }
}