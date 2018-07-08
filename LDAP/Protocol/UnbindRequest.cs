using System;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal sealed class UnbindRequest : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.UNBIND_REQUEST;
        public override bool HasResponse => false;

        protected override async Task WriteContentsAsync(LDAPWriter writer) =>
            // Unbind is [APPLICATION 2] NULL
            await writer.WriteNullAsync(2, EncodingScope.APPLICATION);

        protected override async Task ReadContentsAsync(LDAPReader reader) =>
            // Can't do this....
            throw new InvalidOperationException("Cannot read a request");
    }
}