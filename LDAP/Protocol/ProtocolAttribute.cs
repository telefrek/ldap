using System;

namespace Telefrek.Security.LDAP.Protocol
{
    /// <summary>
    /// Attribute for delivering messages
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class ProtocolAttribute : Attribute
    {
        /// <summary>
        /// The protocol operation type
        /// </summary>
        public ProtocolOp Op { get; set; }

        /// <summary>
        /// Optional Control Operation ID
        /// </summary>
        public string OID { get; set; }
    }
}