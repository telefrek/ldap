namespace Telefrek.LDAP.Protocol.Encoding
{
    /// <summary>
    /// The scope of the encoding
    /// </summary>
    public enum EncodingScope
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