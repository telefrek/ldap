using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol;

namespace Telefrek.LDAP.IO
{
    internal interface ILDAPConnection : IDisposable
    {
        Task ConnectAsync(string host, int port);
        Task CloseAsync();
        Task<ICollection<ProtocolOperation>> TryQueueOperation(ProtocolOperation op, CancellationToken token);
        LDAPReader Reader { get; }
        LDAPWriter Writer { get; }
    }
}