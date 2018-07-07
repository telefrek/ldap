using System;
using System.Threading.Tasks;
using Telefrek.Security.LDAP.Protocol;

namespace Telefrek.Security.LDAP.IO
{
    internal interface ILDAPConnection : IDisposable
    {
        Task ConnectAsync(string host, int port);
        Task CloseAsync();
        Task<bool> TryQueueOperation(ProtocolOperation op);
    }
}