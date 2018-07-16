namespace Telefrek.LDAP
{

    /// <summary>
    /// The type of filter to apply to the attribute
    /// </summary>
    public enum LDAPFilterType
    {
        /// <value></value>
        Add = 0,
        /// <value></value>
        Or = 1,
        /// <value></value>
        Not = 2,
        /// <value></value>
        EqualityMatch = 3,
        /// <value></value>
        Substring = 4,
        /// <value></value>
        GreaterOrEqual = 5,
        /// <value></value>
        LessOrEqual = 6,
        /// <value></value>
        Present = 7,
        /// <value></value>
        ApproximateMatch = 8,
        /// <value></value>
        ExtensibleMatch = 9,
    }
}