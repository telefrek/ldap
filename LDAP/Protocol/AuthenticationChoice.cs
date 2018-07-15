using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{

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