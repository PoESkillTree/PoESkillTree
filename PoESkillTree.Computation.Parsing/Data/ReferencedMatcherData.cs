using System.Diagnostics;

namespace PoESkillTree.Computation.Parsing.Data
{
    /// <summary>
    /// Data that specifies an arbitrary object that can be referenced (and cast using 
    /// <see cref="Builders.Matching.IReferenceConverter"/>) if a stat line matches the <see cref="Regex"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class ReferencedMatcherData
    {
        /// <summary>
        /// The regex pattern that is used for matching.
        /// </summary>
        /// <remarks>
        /// There are a few additional rules this regex pattern must satisfy (see 
        /// <see cref="Referencing.ReferenceValidator"/>):
        /// <list type="bullet">
        /// <item>It must not contain <see cref="Referencing.ReferenceConstants.ValuePlaceholder"/>.</item>
        /// <item>It must not match <see cref="Referencing.ReferenceConstants.ReferencePlaceholderRegex"/>.</item>
        /// <item>Regex group names must not start with <see cref="Referencing.ReferenceConstants.ValueGroupPrefix"/>
        /// or <see cref="Referencing.ReferenceConstants.ReferenceGroupPrefix"/>.</item>
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