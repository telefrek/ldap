namespace Telefrek.LDAP
{
    /// <summary>
    /// Base class for all LDAP entities
    /// </summary>
    public class LDAPObject
    {
        /// <summary>
        /// Gets/Sets the object DistinguishedName (DN)
        /// </summary>
        public string DistinguishedName { get; set; }
    }
}