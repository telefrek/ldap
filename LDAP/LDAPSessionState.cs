namespace Telefrek.LDAP
{
    /// <summary>
    /// LDAPSession state flags
    /// </summary>
    public enum LDAPSessionState
    {
        /// <value></value>
        Closed = 0,
        /// <value></value>
        Open = 1,
        /// <value></value>
        Bound = 3,
    }
}