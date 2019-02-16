using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Collection of <see cref="MatcherData"/>. These may contain (non-circular) references to other
    /// <see cref="IStatMatchers"/> or to <see cref="IReferencedMatchers"/>.
    /// </summary>
    public interface IStatMatchers
    {
        IReadOnlyList<MatcherData> Data { get; }

        /// <summary>
        /// The names by which this instance can be referenced from other <see cref="IStatMatchers"/>. Empty if
        /// this instance can't be referenced. These names may be shared between <see cref="IStatMatchers"/> instances
        /// to allow one name to reference multiple <see cref="IStatMatchers"/> instances.
        /// </summary>
        IReadOnlyList<string> ReferenceNames { get; }

        /// <summary>
        /// If true, <see cref="MatcherData.Regex"/> must match the whole (remaining) stat line.
        /// </summary>
        bool MatchesWholeLineOnly { get; }
    }
}