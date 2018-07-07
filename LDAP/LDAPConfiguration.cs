namespace Telefrek.Security.LDAP
{
    /// <summary>
    /// POCO options class to use with IOptions pattern
    /// </summary>
    public class LDAPOptions
    {
        /// <summary>
        /// The host to connect to ("0.0.0.0")
        /// </summary>
        public string Host { get; set; } = "0.0.0.0";

        /// <summary>
        /// If the connection is secured via TLS (true)
        /// </summary>
        public bool IsSecured { get; set; } = true;

        /// <summary>
        /// The port to use for communicating (636 secure spec port)
        /// </summary>
        public int Port { get; set; } = 636;
    }
}