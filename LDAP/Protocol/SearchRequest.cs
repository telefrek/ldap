using System;
using System.IO;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class SearchRequest : LDAPRequest
    {
        public override ProtocolOp Operation => ProtocolOp.SEARCH_REQUEST;

        public string DistinguishedName { get; set; }
        public LDAPScope Scope { get; set; }
        public LDAPAliasDereferencing Aliasing { get; set; }
        public int SizeLimit { get; set; } = 100;
        public int TimeLimit { get; set; } = 60;
        public bool TypesOnly { get; set; }
        public LDAPFilter Filter { get; set; } = LDAPFilter.ALL_OBJECTS;
        public string[] Attributes { get; set; }

        public override bool HasResponse => true;

        protected override async Task WriteContentsAsync(LDAPWriter writer)
        {
            var opWriter = new LDAPWriter();

            await opWriter.WriteAsync(DistinguishedName);
            await opWriter.WriteAsync((int)Scope);
            await opWriter.WriteAsync((int)Aliasing);
            await opWriter.WriteAsync(SizeLimit);
            await opWriter.WriteAsync(TimeLimit);
            await opWriter.WriteAsync(TypesOnly);

            // Encode filters here
            await opWriter.WriteAsync(Filter);

            // Attributes
            if (Attributes == null || Attributes.Length == 0)
                await opWriter.WriteNullAsync();
            else
            {
                var atWriter = new LDAPWriter();
                foreach (var s in Attributes)
                    await atWriter.WriteAsync(s);

                await opWriter.WriteAsync(atWriter);
            }

            await writer.WriteAsync(opWriter, 3, EncodingScope.APPLICATION);
        }
    }
}