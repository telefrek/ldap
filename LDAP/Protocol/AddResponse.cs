using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    internal class AddResponse : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.ADD_RESPONSE;

        protected override Task ReadResponseAsync(LDAPReader reader) => Task.CompletedTask;
    }
}