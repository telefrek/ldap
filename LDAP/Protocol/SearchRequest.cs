using System;
using System.IO;
using System.Threading.Tasks;

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
            await opWriter.WriteAsync("cn", 7, EncodingScope.CONTEXT_SPECIFIC);
            await opWriter.WriteNullAsync(); // attributes

            await writer.WriteAsync(opWriter, 3, EncodingScope.APPLICATION);
        }
    }
}