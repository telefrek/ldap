namespace Telefrek.LDAP
{
    /// <summary>
    /// Indicates how alias dereferencing should be performed
    /// </summary>
    public enum LDAPAliasDereferencing
    {
        /// <value>Don't dereference ever</value>
        Never = 0,
        /// <value>Dereference during search evaluation</value>
        InSearch = 1,
        /// <value>Dereference while finding base objects</value>
        BaseObject = 2,
        /// <value>Always dereference</value>
        Always = 3,
    }
}