using System;
using System.Text;

namespace Telefrek.LDAP.Managers
{
    /// <summary>
    /// Class used to represent a user in LDAP
    /// </summary>
    public class LDAPUser
    {
        LDAPObject _wrappedObj;

        /// <summary>
        /// Default constructor
        /// </summary>
        public LDAPUser()
        {
            _wrappedObj = null;
        }

        /// <summary>
        /// Constructor from existing user object
        /// </summary>
        /// <param name="userObj">The LDAPObject for the user</param>
        public LDAPUser(LDAPObject userObj)
        {
            _wrappedObj = userObj;

            // Use builders for string manipulation
            var nameBuilder = new StringBuilder();
            var domainBuilder = new StringBuilder();

            // Parse the distinguished name
            foreach (var subStr in userObj.DistinguishedName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var tokens = subStr.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 2)
                {
                    switch (tokens[0].ToLowerInvariant())
                    {
                        case "cn":
                            nameBuilder.Append(tokens[1]);
                            break;
                        case "dc":
                            if (domainBuilder.Length > 0)
                                domainBuilder.Append(".");
                            domainBuilder.Append(tokens[1]);
                            break;
                        default:
                            break;
                    }
                }
            }

            Name = nameBuilder.ToString();
            Domain = domainBuilder.ToString();
        }

        /// <summary>
        /// Gets/Sets the user name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/Sets the user domain
        /// </summary>
        public string Domain { get; set; }
    }
}