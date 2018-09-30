using System;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class BindResponse : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.BIND_RESPONSE;
    }
}