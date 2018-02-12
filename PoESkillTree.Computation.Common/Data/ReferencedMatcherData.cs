using System.Diagnostics;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Data that specifies an arbitrary object that can be referenced (and cast using 
    /// <see cref="Common.Builders.Resolving.IReferenceConverter"/>) if a stat line matches the <see cref="Regex"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class ReferencedMatcherData
    {
        /// <summary>
        /// The regex pattern that is used for matching.
        /// </summary>
        /// <remarks>
        /// There are a few additional rules this regex pattern must satisfy:
        /// <list type="bullet">
        /// <item>It must not contain <see cref="ReferenceConstants.ValuePlaceholder"/>.</item>
        /// <item>It must not match <see cref="ReferenceConstants.ReferencePlaceholderRegex"/>.</item>
        /// <item>Regex group names must not start with <see cref="ReferenceConstants.ValueGroupPrefix"/>
        /// or <see cref="ReferenceConstants.ReferenceGroupPrefix"/>.</item>
        /// </list>
        /// <para>The regex pattern is used with the IgnoreCase, CultureInvariant and ExplicitCapture options.</para>
        /// </remarks>
        public string Regex { get; }

        /// <summary>
        /// The object that can be referenced if the regex matches the remaining stat line
        /// and the regex is longest between all matching <see cref="ReferencedMatcherData"/>.
        /// </summary>
        public object Match { get; }

        public ReferencedMatcherData(string regex, object match)
        {
            Regex = regex;
            Match = match;
        }
    }
}