namespace Telefrek.LDAP
{
    /// <summary>
    /// Class for tracking substring filters
    /// </summary>
    public class LDAPSubstringFilter
    {
        /// <summary>
        /// Gets/Sets the substring filter type
        /// </summary>
        public LDAPSubstringType SubstringType { get; set; }

        /// <summary>
        /// Gets/Sets the filter value
        /// </summary>
        public string Value { get; set; }
    }
}