using System.Collections.Generic;

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

        /// <summary>
        /// Gets/Sets the list of attributes for the object
        /// </summary>
        public List<LDAPAttribute> Attributes { get; set; } = new List<LDAPAttribute>();
    }
}