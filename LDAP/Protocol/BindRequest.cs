using System.IO;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    /// <summary>
    ///  Performs a bind request against an LDAP server
    /// 
    ///  BindRequest ::= [APPLICATION 0] SEQUENCE {
    ///  version                 INTEGER (1 ..  127),
    ///  name                    LDAPDN,
    ///  authentication          AuthenticationChoice }
    /// 
    /// </summary>
    internal class BindRequest : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.BIND_REQUEST;

        public int Version { get; set; } = 3;
        public string Name { get; set; }
        public AuthenticationChoice Authentication { get; set; }

        protected override async Task WriteContentsAsync(Stream target)
        {
            var ms = new MemoryStream();

            await ProtocolEncoding.WriteAsync(ms, Version);
            await ProtocolEncoding.WriteAsync(ms, Name);
            await Authentication.WriteAsync(ms);

            ms.Seek(0, SeekOrigin.Begin);
            await ProtocolEncoding.WriteAsync(target, ms, 0, (int)EncodingScope.APPLICATION);
        }
    }

    /// <summary>
    /// Authentication choice used for identifying the user
    /// 
    /// AuthenticationChoice ::= CHOICE {
    /// simple                  [0] OCTET STRING,
    ///                         -- 1 and 2 reserved
    /// sasl                    [3] SaslCredentials,
    /// ...  }
    /// </summary>
    internal abstract class AuthenticationChoice
    {
        public abstract Task WriteAsync(Stream target);
    }

    internal class SimpleAuthentication : AuthenticationChoice
    {
        public string Credentials { get; set; }

        public override async Task WriteAsync(Stream target)
        {
            // Tag for simple is 0
            await ProtocolEncoding.WriteAsync(target, Credentials, 0, (int)EncodingScope.CONTEXT_SPECIFIC);
        }
    }
}