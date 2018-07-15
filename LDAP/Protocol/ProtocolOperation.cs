using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    /// <summary>
    /// Default protocol operation class used to pass messages back and forth
    /// </summary>
    internal abstract class ProtocolOperation
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
                    return await ReadOperation<BindResponse>(messageReader, messageId);
                case ProtocolOp.SEARCH_RESPONSE:
                    return await ReadOperation<SearchResponse>(messageReader, messageId);
                case ProtocolOp.SEARCH_RESULT:
                    return await ReadOperation<SearchResult>(messageReader, messageId);
                case ProtocolOp.ADD_RESPONSE:
                    return await ReadOperation<AddResponse>(messageReader, messageId);
                case ProtocolOp.DEL_RESPONSE:
                    return await ReadOperation<DeleteResponse>(messageReader, messageId);
                default:
                    throw new LDAPProtocolException("Unknown/Invalid protocol operation");
            }
        }

        private static async Task<T> ReadOperation<T>(LDAPReader messageReader, int messageId) where T : ProtocolOperation, new()
        {
            var op = new T() { MessageId = messageId };
            await op.ReadContentsAsync(messageReader);

            return op;
        }

        protected abstract Task WriteContentsAsync(LDAPWriter writer);

        protected abstract Task ReadContentsAsync(LDAPReader reader);
    }
}