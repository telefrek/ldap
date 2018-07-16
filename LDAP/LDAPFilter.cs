using System.Collections.Generic;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Represents a filter to be applied on an LDAP search
    /// </summary>
    public class LDAPFilter
    {
        /// <summary>
        /// Static filter for all objects
        /// </summary>
        public static readonly LDAPFilter ALL_OBJECTS = new LDAPFilter();

        /// <summary>
        /// Gets the filter type
        /// </summary>
        public LDAPFilterType FilterType { get; set; } = LDAPFilterType.Present;

        /// <summary>
        /// Gets/Sets the filter value
        /// </summary>
        public string Value { get; set; } = "*";

        /// <summary>
        /// Gets/Sets the target attribute
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/Sets the substring filters
        /// </summary>
        public List<LDAPSubstringFilter> Substrings { get; set; }
    }
}