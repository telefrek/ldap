using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class ModifyRequest : LDAPRequest
    {
        public override ProtocolOp Operation => ProtocolOp.MODIFY_REQUEST;

        public string DistinguishedName { get; set; }
        public LDAPAttribute[] Removed { get; set; }
        public LDAPAttribute[] Modified { get; set; }
        public LDAPAttribute[] Added { get; set; }

        protected override async Task WriteContentsAsync(LDAPWriter writer)
        {
            var opWriter = new LDAPWriter();

            await opWriter.WriteAsync(DistinguishedName);
            var modWriter = new LDAPWriter();

            foreach (var attr in Added ?? new LDAPAttribute[0])
            {
                var w = new LDAPWriter();
                await w.WriteAsync(attr);
                await modWriter.WriteAsync(w, 0, EncodingScope.CONTEXT_SPECIFIC);
            }

            foreach (var attr in Removed ?? new LDAPAttribute[0])
            {
                var w = new LDAPWriter();
                await w.WriteAsync(attr);
                await modWriter.WriteAsync(w, 1, EncodingScope.CONTEXT_SPECIFIC);
            }

            foreach (var attr in Modified ?? new LDAPAttribute[0])
            {
                var w = new LDAPWriter();
                await w.WriteAsync(attr);
                await modWriter.WriteAsync(w, 2, EncodingScope.CONTEXT_SPECIFIC);
            }

            await opWriter.WriteAsync(modWriter);

            await writer.WriteAsync(opWriter, (int)Operation, EncodingScope.APPLICATION);
        }
    }
}