using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    internal class DeleteResponse : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.DEL_RESPONSE;

        protected override Task ReadResponseAsync(LDAPReader reader) => Task.CompletedTask;
    }
}