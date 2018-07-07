namespace Telefrek.Security.LDAP.Protocol.BER
{
    /// <summary>
    /// Represents the types of BER classes
    /// </summary>
    public enum BERClass
    {
        /// <value>Valid for any ASN.1</value>
        UNIVERSAL = 0,
        /// <value>Specific to the given application</value>
        APPLICATION = 1,
        /// <value>Depends on context</value>
        CONTEXT_SPECIFIC = 2,
        /// <value>Customization for a given app</value>
        PRIVATE = 3,
    }
}