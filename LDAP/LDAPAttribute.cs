using System.Collections.Generic;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Represents an LDAP attribute object
    /// </summary>
    public class LDAPAttribute
    {
        /// <summary>
        /// Gets/Sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/Sets the values
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();
    }
}