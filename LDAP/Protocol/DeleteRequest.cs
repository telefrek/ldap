using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class DeleteRequest : LDAPRequest
    {
        public override ProtocolOp Operation => ProtocolOp.DEL_REQUEST;

        public string DistinguishedName { get; set; }

        protected override async Task WriteContentsAsync(LDAPWriter writer) => await writer.WriteAsync(DistinguishedName, 10, EncodingScope.APPLICATION);
    }
}