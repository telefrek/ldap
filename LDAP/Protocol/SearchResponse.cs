using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    internal class SearchResponse : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.SEARCH_RESPONSE;

        public override bool IsTerminating => false;

        public string DistinguishedName { get; set; }
        public LDAPAttribute[] Attributes { get; set; }

        protected override async Task ReadContentsAsync(LDAPReader reader)
        {
            var msgReader = reader.CreateReader();

            await msgReader.ReadAsync();

            DistinguishedName = await reader.ReadAsStringAsync();

            if(await msgReader.ReadAsync())
            {
                var attrReader = msgReader.CreateReader();
                var attrList = new List<LDAPAttribute>();
                while(await attrReader.ReadAsync())
                {
                    var attr = new LDAPAttribute();
                    await attr.ReadContentsAsync(attrReader);
                }
            }
        }

        protected override Task WriteContentsAsync(LDAPWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}