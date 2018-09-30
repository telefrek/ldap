using System.Collections.Generic;
using System.Linq;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Base class for all LDAP entities
    /// </summary>
    public class LDAPObject
    {
        /// <summary>
        /// Gets/Sets the object DistinguishedName (DN)
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// The root domain for the object
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets/Sets the list of attributes for the object
        /// </summary>
        public List<LDAPAttribute> Attributes { get; set; } = new List<LDAPAttribute>();

        /// <summary>
        /// Clones the current object to another, identical entity
        /// </summary>
        /// <returns>A new copy of the object and all it's properties</returns>
        public LDAPObject Clone()
        {
            // Create clone of current
            var clone = new LDAPObject
            {
                DistinguishedName = DistinguishedName,
                Domain = Domain,
            };

            // Force copy of objects (slow but prevents references)
            clone.Attributes.AddRange(Attributes.Select(a =>
            {
                var a1 = new LDAPAttribute { Description = a.Description };
                a1.Values.AddRange(a.Values);

                return a1;
            }));

            return clone;
        }
    }
}