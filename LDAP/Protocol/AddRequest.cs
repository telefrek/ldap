using System.IO;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    internal class AddRequest : LDAPRequest
    {
        public override ProtocolOp Operation => ProtocolOp.ADD_REQUEST;

        public string DistinguishedName { get; set; }
        public LDAPAttribute[] Attributes { get; set; }

        protected override async Task WriteContentsAsync(LDAPWriter writer)
        {
            var opWriter = new LDAPWriter();

            await opWriter.WriteAsync(DistinguishedName);

            var attrWriter = new LDAPWriter();
            foreach (var attr in Attributes ?? new LDAPAttribute[] { })
                await attr.WriteContentsAsync(attrWriter);

            await opWriter.WriteAsync(attrWriter);

            await writer.WriteAsync(opWriter, (int)Operation, EncodingScope.APPLICATION);
        }
    }
}