using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal sealed class UnbindRequest : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.UNBIND_REQUEST;

        protected override async Task WriteContentsAsync(LDAPWriter target) => 
            // Unbind is [APPLICATION 2] NULL
            await target.WriteNullAsync(2, EncodingScope.APPLICATION);
    }
}