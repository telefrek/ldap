using System.Collections.Generic;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Represents a generic LDAP Result that applies to all messages
    /// </summary>
    public class LDAPResult
    {
        /// <summary>
        /// Gets/Sets the success flag
        /// </summary>
        public bool WasSuccessful { get; set; }
        /// <summary>
        /// Gets/Sets the actual result code from the LDAP server
        /// </summary>
        public LDAPResultCode ResultCode { get; set; }
        /// <summary>
        /// Gets/Sets the diagnostic message (if provided) from the LDAP server
        /// </summary>
        public string DiagnosticMessage { get; set; }
        /// <summary>
        /// Gets/Sets the objects returned from the server
        /// </summary>
        public IEnumerable<LDAPObject> Objects { get; set; }
        /// <summary>
        /// Gets/Sets the flag to indicate that objects are being streamed or delivered on completion
        /// </summary>
        public bool IsStreaming { get; set; }
    }
}