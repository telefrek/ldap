using System;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal abstract class LDAPRequest : ProtocolOperation
    {
        protected override Task ReadContentsAsync(LDAPReader reader) => throw new InvalidOperationException("Cannot read a request");
    }
}