namespace Telefrek.LDAP.Protocol
{
    internal class ModifyResponse : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.MODIFY_RESPONSE;
    }
}