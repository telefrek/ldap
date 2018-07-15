using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    internal class SearchResult : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.SEARCH_RESULT;

        protected override Task ReadResponseAsync(LDAPReader reader) => Task.CompletedTask;
    }
}