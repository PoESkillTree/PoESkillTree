using System;
using System.Text.RegularExpressions;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
#if (PoESkillTree_UseSmallDec_ForAttributes)
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#endif
    /// <summary>
    /// Data class describing a conditioned Attribute.
    /// At least one condition must be true or there must not be
    /// any conditions for <see cref="ICondition.Evaluate"/> to return true.
    /// (see <see cref="OrComposition"/>)
    /// </summary>
    public class Attribute : OrComposition
    {
        private static readonly Regex WildcardRegex = new Regex(@"{\d+}");

        /// <summary>
        /// Gets the name this attribute. 
        /// Represents the name of an attribute of nodes in the skill tree
        /// (numbers replaced by '#').
        /// </summary>
        /// <remarks>
        /// Parts of the skill node attribute may be replaced by '{number}'.
        /// For testing if a skill node attribute matches this attribute, these
        /// wildcards can match any string. The wildcards can be referenced
        /// in the conditions: '{number}' parts in conditions are replaced by
        /// all matches of skill node attributes.
        /// </remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the multiplier by which values of skill node attributes should
        /// be multiplied before being adding into the <see cref="PseudoAttribute"/>
        /// containing this attribute.
        /// </summary>
        public
#if (PoESkillTree_UseSmallDec_ForAttributes)
        SmallDec
#else
        float
#endif
        ConversionMultiplier { get; internal set; }

        /// <summary>
        /// Creates a new Attribute with the given name, a ConversionMultiplier
        /// of 1 and no conditions.
        /// </summary>
        /// <param name="name">Name referencing skill tree attributes (not null)</param>
        internal Attribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            ConversionMultiplier = 1;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns true if <paramref name="actualAttributeName"/> matches this attribute's name
        /// (while taking wildcards into account) and evaluates to true given <paramref name="settings"/>
        /// and the replacements if applicable.
        /// </summary>
        /// <param name="settings">The settings to evaluate under. (not null)</param>
        /// <param name="actualAttributeName">The attribute name to compare <see cref="Name"/> against. (not null)</param>
        /// <returns>
        /// True if <paramref name="actualAttributeName"/> matches <see cref="Name"/> and <see cref="ICondition.Evaluate"/>
        /// returns true.
        /// </returns>
        public bool MatchesAndEvaluates(ConditionSettings settings, string actualAttributeName)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (actualAttributeName == null) throw new ArgumentNullException("actualAttributeName");

            // If the attribute name has no wildcards, check if it matches the actual name
            // and evaluate without replacements.
            if (!WildcardRegex.IsMatch(Name))
                return Name == actualAttributeName && Evaluate(settings);

            var searchRegex = new Regex("^" + WildcardRegex.Replace(Name, "(.*)") + "$");
            var match = searchRegex.Match(actualAttributeName);
            if (!match.Success) return false;

            var groups = match.Groups;
            var groupNames = new string[groups.Count - 1];
            // The first group is the whole match, we don't want that one.
            for (int i = 0, j = 1; j < groups.Count; i++, j++)
            {
                groupNames[i] = groups[j].Value;
            }
            return Evaluate(settings, groupNames);
        }
    }
}