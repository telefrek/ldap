using System;
using System.Collections.Generic;
using System.Linq;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Represents a filter to be applied on an LDAP search
    /// </summary>
    public class LDAPFilter
    {
        /// <summary>
        /// Static filter for all objects
        /// </summary>
        public static readonly LDAPFilter ALL_OBJECTS = new LDAPFilter();

        /// <summary>
        /// Static filter for all cn
        /// </summary>
        public static readonly LDAPFilter ALL_CN = new LDAPFilter { Description = "cn" };

        /// <summary>
        /// Gets the filter type
        /// </summary>
        public LDAPFilterType FilterType { get; set; } = LDAPFilterType.Present;

        /// <summary>
        /// Gets/Sets the filter value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets/Sets the target attribute
        /// </summary>
        public string Description { get; set; } = "objectClass";

        /// <summary>
        /// Gets/Sets the matching rule to apply
        /// </summary>
        public string MatchingRule { get; set; }

        /// <summary>
        /// Gets/Sets the all attributes flag for extensible filters
        /// </summary>
        public bool AllAttributes { get; set; }

        /// <summary>
        /// Gets/Sets the substring filters
        /// </summary>
        public List<LDAPSubstringFilter> Substrings { get; set; }

        /// <summary>
        /// Gets/Sets the child filters
        /// </summary>
        public List<LDAPFilter> Children { get; set; } = new List<LDAPFilter>();

        /// <summary>
        /// Not operator overload
        /// </summary>
        /// <param name="original">The original filter to negate</param>
        /// <returns>A new negated filter</returns>
        public static LDAPFilter operator ~(LDAPFilter original)
        {
            return new LDAPFilter
            {
                FilterType = LDAPFilterType.Not,
                Children = new List<LDAPFilter>() { original }
            };
        }

        /// <summary>
        /// And operator overload
        /// </summary>
        /// <param name="left">The left predicate</param>
        /// <param name="right">The right predicate</param>
        /// <returns>A new and filter</returns>
        public static LDAPFilter operator &(LDAPFilter left, LDAPFilter right)
        {
            return new LDAPFilter
            {
                FilterType = LDAPFilterType.Add,
                Children = new List<LDAPFilter>() { left, right }
            };
        }

        /// <summary>
        /// Or operator overload
        /// </summary>
        /// <param name="left">The left predicate</param>
        /// <param name="right">The right predicate</param>
        /// <returns>A new or filter</returns>
        public static LDAPFilter operator |(LDAPFilter left, LDAPFilter right)
        {
            return new LDAPFilter
            {
                FilterType = LDAPFilterType.Or,
                Children = new List<LDAPFilter>() { left, right }
            };
        }

        /// <summary>
        /// Creates a prefix filter for the attribute
        /// </summary>
        /// <param name="attribute">The target attribute</param>
        /// <param name="value">The prefix value</param>
        /// <returns>A new filter</returns>
        public static LDAPFilter CreatePrefix(string attribute, string value)
        {
            return new LDAPFilter
            {
                FilterType = LDAPFilterType.Substring,
                Substrings = new List<LDAPSubstringFilter>()
                {
                    new LDAPSubstringFilter
                    {
                        SubstringType = LDAPSubstringType.Initial,
                        Value = value,
                    }
                }
            };
        }

        /// <summary>
        /// Creates a suffix filter for the attribute
        /// </summary>
        /// <param name="attribute">The target attribute</param>
        /// <param name="value">The suffix value</param>
        /// <returns>A new filter</returns>
        public static LDAPFilter CreateSuffix(string attribute, string value)
        {
            return new LDAPFilter
            {
                FilterType = LDAPFilterType.Substring,
                Substrings = new List<LDAPSubstringFilter>()
                {
                    new LDAPSubstringFilter
                    {
                        SubstringType = LDAPSubstringType.Final,
                        Value = value,
                    }
                }
            };
        }

        /// <summary>
        /// Creates a wildcard filter for the attribute
        /// </summary>
        /// <param name="attribute">The target attribute</param>
        /// <param name="value">The wildcard value (no * characters necessary)</param>
        /// <returns>A new filter</returns>
        public static LDAPFilter CreateWildcard(string attribute, string value)
        {
            return new LDAPFilter
            {
                FilterType = LDAPFilterType.Substring,
                Substrings = new List<LDAPSubstringFilter>()
                {
                    new LDAPSubstringFilter
                    {
                        SubstringType = LDAPSubstringType.Any,
                        Value = value,
                    }
                }
            };
        }
    }

    /// <summary>
    /// Extensions for fiter combinations
    /// </summary>
    public static class LDAPFilterExtensions
    {
        /// <summary>
        /// Tries to combine two substring filters
        /// </summary>
        /// <param name="filter">The original substring filter</param>
        /// <param name="addition">The second substring filter to add</param>
        /// <returns>A new filter, if the combination is valid</returns>
        public static LDAPFilter TryCombine(this LDAPFilter filter, LDAPFilter addition)
        {
            if (filter == null)
                throw new ArgumentNullException("filter was null");
            if (addition == null)
                throw new ArgumentNullException("addition was null");

            // Validate the filter type
            if (filter.FilterType == LDAPFilterType.Substring && addition.FilterType == filter.FilterType)
            {
                // Validate they both have substrings
                if (filter.Substrings.Count > 0 && addition.Substrings.Count > 0)
                {
                    if (filter.Substrings.Count(s => s.SubstringType == LDAPSubstringType.Initial) +
                        filter.Substrings.Count(s => s.SubstringType == LDAPSubstringType.Initial) > 1)
                        return null;

                    if (filter.Substrings.Count(s => s.SubstringType == LDAPSubstringType.Final) +
                        filter.Substrings.Count(s => s.SubstringType == LDAPSubstringType.Final) > 1)
                        return null;

                    var combined = new LDAPFilter
                    {
                        FilterType = LDAPFilterType.Substring
                    };

                    foreach (var sub in filter.Substrings)
                        combined.Substrings.Add(sub);

                    foreach (var sub in addition.Substrings)
                        combined.Substrings.Add(sub);

                    return combined;
                }
            }

            return null;
        }
    }
}