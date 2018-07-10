using System;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal sealed class UnbindRequest : LDAPRequest
    {
        public override ProtocolOp Operation => ProtocolOp.UNBIND_REQUEST;
        public override bool HasResponse => false;

        protected override async Task WriteContentsAsync(LDAPWriter writer) =>
            await writer.WriteNullAsync(2, EncodingScope.APPLICATION);
    }
}