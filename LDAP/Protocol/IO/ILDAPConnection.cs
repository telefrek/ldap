using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol.IO
{
    internal interface ILDAPConnection : IDisposable
    {
        Task ConnectAsync(string host, int port);
        Task CloseAsync();
        Task<IEnumerable<LDAPResponse>> TryQueueOperation(LDAPRequest request, CancellationToken token);
        LDAPReader Reader { get; }
        LDAPWriter Writer { get; }
    }
}