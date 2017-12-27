using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// Constants related to placeholders and group names for references.
    /// </summary>
    /// <remarks>
    /// Used in <see cref="RegexGroupService"/> and <see cref="StatMatcherRegexExpander"/>. 
    /// <see cref="ReferenceValidator"/> makes sure the parsing data uses these correctly.
    /// </remarks>
    public static class ReferenceConstants
    {
        /// <summary>
        /// Placeholder for arbitrary integer or decimal values in <see cref="Data.MatcherData.Regex"/>. These will
        /// be expanded to regexes matching integer or decimal values.
        /// </summary>
        public const string ValuePlaceholder = "#";
        /// <summary>
        /// <see cref="ValuePlaceholder"/> as a <see cref="Regex"/>.
        /// </summary>
        public static readonly Regex ValuePlaceholderRegex = new Regex(ValuePlaceholder);
        /// <summary>
        /// A <see cref="Regex"/> for the format references in <see cref="Data.MatcherData.Regex"/> must have to
        /// be expanded to regexes matching the referenced matchers.
        /// </summary>
        public static readonly Regex ReferencePlaceholderRegex = new Regex(@"\(\{(\w+)\}\)");

        /// <summary>
        /// Prefix for regex group names for expanded value placeholders. Regex groups in 
        /// <see cref="Data.MatcherData.Regex"/> must not start with this.
        /// </summary>
        public const string ValueGroupPrefix = "value";
        /// <summary>
        /// Prefix for regex group names for expanded reference placeholders. Regex groups in 
        /// <see cref="Data.MatcherData.Regex"/> must not start with this.
        /// </summary>
        public const string ReferenceGroupPrefix = "reference";
    }
}