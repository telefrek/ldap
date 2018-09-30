namespace Telefrek.LDAP.Protocol.IO
{
    /// <summary>
    /// Internal state enum for tracking the connection state
    /// </summary>
    internal enum LDAPConnectionState
    {
        NotInitialized = 0,
        Connected = 1,
        Closed = 2,
        Faulted = 3,
    }
}