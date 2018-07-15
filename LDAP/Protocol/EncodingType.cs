namespace Telefrek.LDAP.Protocol
{
    /// <summary>
    /// Type of value being encoded
    /// </summary>
    public enum EncodingType
    {
         /// <value>End of contents</value>
        END_OF_CONTENT = 0x0,
        /// <value>Boolean value</value>
        BOOLEAN = 0x1,
        /// <value>Int32 value</value>
        INTEGER = 0x2,
        /// <value>Collection of bits</value>
        BIT_STRING = 0x3,
        /// <value>Collection of octets</value>
        OCTET_STRING = 0x4,
        /// <value>Empty value</value>
        NULL = 0x5,
        /// <value>Object identifier</value>
        OBJECT_IDENTIFIER = 0x6,
        /// <value>Object description</value>
        OBJECT_DESCRIPTOR = 0x7,
        /// <value>External type</value>
        EXTERNAL = 0x8,
        /// <value>Float</value>
        REAL = 0x9,
        /// <value>Enums</value>
        ENUMERATED = 0xA,
        /// <value></value>
        EMBEDDED_PDV = 0xB,
        /// <value>UTF8 encoded string</value>
        UTF8_STRING = 0xC,
        /// <value></value>
        RELATIVE_OID = 0xD,
        /// <value>Collection of values</value>
        SEQUENCE = 0x10,
        /// <value>Set of values</value>
        SET = 0x11,
        /// <value></value>
        NUMERIC_STRING = 0x12,
        /// <value></value>
        PRINTABLE_STRING = 0x13,
        /// <value></value>
        T61_STRING = 0x14,
        /// <value></value>
        VIDEO_TEX_STRING = 0x15,
        /// <value></value>
        IA5_STRING = 0x16,
        /// <value></value>
        UTC_TIME = 0x17,
        /// <value></value>
        GENERALIZED_TIME = 0x18,
        /// <value></value>
        GRAPHIC_STRING = 0x19,
        /// <value></value>
        VISIBLE_STRING = 0x1A,
        /// <value></value>
        GENERAL_STRING = 0x1B,
        /// <value></value>
        UNIVERSAL_STRING = 0x1C,
        /// <value></value>
        CHARACTER_STRING = 0x1D,
        /// <value></value>
        BMP_STRING = 0x1E,
    }
}