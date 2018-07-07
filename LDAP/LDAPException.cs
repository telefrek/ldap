using System.Runtime.Serialization;

namespace Telefrek.Security.LDAP
{
    /// <summary>
    /// Base LDAP exception
    /// </summary>
    [System.Serializable]
    public class LDAPException : System.Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LDAPException() { }

        /// <summary>
        /// Create an exception with the given message
        /// </summary>
        /// <param name="message">The message to associate with the exception</param>
        public LDAPException(string message) : base(message) { }

        /// <summary>
        /// Create an exception with the given message and root cause
        /// </summary>
        /// <param name="message">The message to associate with the exception</param>
        /// <param name="inner">The exception associated with this instance</param>
        public LDAPException(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>
        /// Inner constructor to use during serialization
        /// </summary>
        /// <param name="info">The serialization information</param>
        /// <param name="context">The current stream context</param>
        protected LDAPException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}