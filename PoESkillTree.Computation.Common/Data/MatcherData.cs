using System.Diagnostics;
using PoESkillTree.Computation.Common.Builders.Modifiers;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Data that specifies an <see cref="IIntermediateModifier"/> that applies if a stat line matches the (expanded)
    /// <see cref="Regex"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class MatcherData
    {
        /// <summary>
        /// The regex pattern that is used for matching. <see cref="ReferenceConstants.ValuePlaceholder"/>
        /// in the pattern are expanded to match arbitrary integer or decimal numbers.
        /// Substrings matching <see cref="ReferenceConstants.ReferencePlaceholderRegex"/> are expanded
        /// to match any (recursively expanded) regex of <see cref="IStatMatchers"/> or 
        /// <see cref="IReferencedMatchers"/> with a name identical to the matched substring's only group.
        /// E.g. a substring "({XyzMatchers})" is expanded to match any regex of the matcher with a reference name
        /// "XyzMatchers".
        /// </summary>
        /// <remarks>
        /// There are a few additional rules this regex pattern must satisfy:
        /// <list type="bullet">
        /// <item>If the <see cref="IStatMatchers"/> instance this instance is part of has reference names, it must not
        /// contain <see cref="ReferenceConstants.ValuePlaceholder"/>.</item>
        /// <item>It may not contain unknown reference names.</item>
        /// <item>It may not (recursively) reference itself.</item>
        /// <item>Regex group names must not start with <see cref="ReferenceConstants.ValueGroupPrefix"/>
        /// or <see cref="ReferenceConstants.ReferenceGroupPrefix"/>.</item>
        /// </list>
        /// <para>The regex pattern is used with the IgnoreCase, CultureInvariant and ExplicitCapture options.</para>
        /// </remarks>
        public string Regex { get; }

        /// <summary>
        /// The <see cref="IIntermediateModifier"/> that is applied if the regex matches the remaining stat line
        /// and it matches the longest substring between all matching <see cref="MatcherData"/>.
        /// <para> If matched as part of an expanded reference: The condition is not being the longest matched substring
        /// having the longest expanded <see cref="Regex"/>.
        /// </para>
        /// </summary>
        public IIntermediateModifier Modifier { get; }

        /// <summary>
        /// Used as parameter for <see cref="System.Text.RegularExpressions.Match.Result"/> to replace the matched
        /// substring in the stat line. References to group names in <see cref="Regex"/> must be to named groups,
        /// groups without names (numbered groups) are not captured (see 
        /// <see cref="System.Text.RegularExpressions.RegexOptions.ExplicitCapture"/>).
        /// </summary>
        public string MatchSubstitution { get; }

        public MatcherData(string regex, IIntermediateModifier modifier, string matchSubstitution = "")
        {
            Regex = regex;
            Modifier = modifier;
            MatchSubstitution = matchSubstitution;
        }
    }
}