using System;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.IO
{
    public interface ILDAPConnection : IDisposable
    {
        Task ConnectAsync(string host, int port);
        Task CloseAsync();
    }
}