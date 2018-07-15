using System;
using System.Runtime.Serialization;

namespace Telefrek.LDAP.Protocol
{
    /// <summary>
    /// Protocol specific exceptions
    /// </summary>
    public class LDAPProtocolException : LDAPException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LDAPProtocolException()
        {
        }

        /// <summary>
        /// Constructor with custom message
        /// </summary>
        /// <param name="message">The message to pass along</param>
        public LDAPProtocolException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor with custom message and cause
        /// </summary>
        /// <param name="message">The message to pass along</param>
        /// <param name="inner">The exception that caused this error</param>
        /// <returns></returns>
        public LDAPProtocolException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}