namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// POCO options class to use with IOptions pattern
    /// </summary>
    public class LDAPManagerConfiguration
    {
        /// <summary>
        /// The administrator account for the LDAP Sessions
        /// </summary>
        public string Administrator { get; set; } = "admin";

        /// <summary>
        /// The primary domain to use for the LDAP Sessions
        /// </summary>
        public string Domain { get; set; } = "example.org";

        /// <summary>
        /// Optional administrator credentials for the LDAP Sessions
        /// </summary>
        public string Credentials { get; set; } = "admin";
    }
}