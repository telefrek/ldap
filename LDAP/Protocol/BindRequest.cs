using System;
using System.IO;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    /// <summary>
    ///  Performs a bind request against an LDAP server to login with specific credentials
    /// </summary>
    internal class BindRequest : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.BIND_REQUEST;

        public int Version { get; set; } = 3;
        public string Name { get; set; }
        public AuthenticationChoice Authentication { get; set; }

        protected override async Task WriteContentsAsync(LDAPWriter writer)
        {
            var opWriter = new LDAPWriter(new MemoryStream());

            await opWriter.WriteAsync(Version);
            await opWriter.WriteAsync(Name);

            await Authentication.WriteAsync(opWriter);

            await writer.WriteAsync(opWriter, 0, EncodingScope.APPLICATION);
        }

        protected override async Task ReadContentsAsync(LDAPReader reader) =>
            // Can't do this....
            throw new InvalidOperationException("Cannot read a request");
    }
}