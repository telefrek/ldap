using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class AddResponse : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.ADD_RESPONSE;
    }
}