using System;

namespace Telefrek.Security.LDAP.Protocol
{
    /// <summary>
    /// Attribute for delivering messages
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Property)]
    public class ProtocolAttribute : Attribute
    {
        /// <summary>
        /// The protocol operation type
        /// </summary>
        /// <value>Default value is none</value>
        public ProtocolOp Op { get; set; } = ProtocolOp.NONE;

        /// <summary>
        /// The type of encoding for this value
        /// </summary>
        /// <value></value>
        public EncodingType Encoding { get; set; }

        /// <summary>
        /// The scope for the value encoding
        /// </summary>
        /// <value>Default is Universal</value>
        public EncodingScope Scope { get; set; } = EncodingScope.UNIVERSAL;

        /// <summary>
        /// Optional Control Operation ID
        /// </summary>
        public string OID { get; set; }
    }
}