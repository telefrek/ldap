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
    }

    /// <summary>
    /// Authentication choice used for identifying the user
    /// </summary>
    internal abstract class AuthenticationChoice
    {
        public abstract Task WriteAsync(LDAPWriter writer);
    }

    /// <summary>
    /// Simple authentication mechanism using plaintext username/password
    /// </summary>
    internal class SimpleAuthentication : AuthenticationChoice
    {
        public string Credentials { get; set; }

        public override async Task WriteAsync(LDAPWriter writer) =>
            // Tag for simple is 0
            await writer.WriteAsync(Credentials, 0, EncodingScope.CONTEXT_SPECIFIC);
    }
}