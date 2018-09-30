using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class SearchResponse : LDAPResponse
    {
        public override ProtocolOp Operation => ProtocolOp.SEARCH_RESPONSE;

        public override bool IsTerminating => false;

        public string DistinguishedName { get; set; }
        public LDAPAttribute[] Attributes { get; set; } = new LDAPAttribute[0];

        public override async Task ReadContentsAsync(LDAPReader reader)
        {
            var msgReader = reader.CreateReader();

            await msgReader.ReadAsync();

            DistinguishedName = await msgReader.ReadAsStringAsync();

            if (await msgReader.ReadAsync())
                Attributes = (await msgReader.ReadPartialListAsync()).ToArray();
        }
    }
}