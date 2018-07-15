namespace Telefrek.LDAP
{
    /// <summary>
    /// Search scope for LDAP operations
    /// </summary>
    public enum LDAPScope
    {
        /// <value>Only the current object scope is examined</value>
        BaseObject = 0,
        /// <value>Scope to the current object and it's immediate children</value>
        SingleLevel = 1,
        /// <value>Search the entire object subtree</value>
        EntireSubtree = 2,
    }
}