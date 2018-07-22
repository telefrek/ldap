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
        public string Value { get; set; }

        /// <summary>
        /// Gets/Sets the target attribute
        /// </summary>
        public string Description { get; set; } = "cn";

        /// <summary>
        /// Gets/Sets the matching rule to apply
        /// </summary>
        public string MatchingRule { get; set; }

        /// <summary>
        /// Gets/Sets the all attributes flag for extensible filters
        /// </summary>
        public bool AllAttributes { get; set; }

        /// <summary>
        /// Gets/Sets the substring filters
        /// </summary>
        public List<LDAPSubstringFilter> Substrings { get; set; }

        /// <summary>
        /// Gets/Sets the child filters
        /// </summary>
        public List<LDAPFilter> Children { get; set; } = new List<LDAPFilter>();
    }
}