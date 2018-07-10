using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal class SearchResponse : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.SEARCH_RESPONSE;

        public override bool IsTerminating => false;

        protected override async Task ReadContentsAsync(LDAPReader reader)
        {
            await reader.SkipAsync();
        }

        protected override Task WriteContentsAsync(LDAPWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}