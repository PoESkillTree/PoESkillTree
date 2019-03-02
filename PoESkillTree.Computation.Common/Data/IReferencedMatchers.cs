using System;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Collection of <see cref="ReferencedMatcherData"/>. These are only used when referenced by 
    /// <see cref="IStatMatchers"/> instances.
    /// </summary>
    public interface IReferencedMatchers
    {
        IReadOnlyList<ReferencedMatcherData> Data { get; }

        /// <summary>
        /// The unique name by which this instance can be referenced from regexes in <see cref="IStatMatchers"/>.
        /// </summary>
        string ReferenceName { get; }

        /// <summary>
        /// All <see cref="ReferencedMatcherData.Match"/> properties in this collection hold values that are instances
        /// of type <see cref="MatchType"/>.
        /// </summary>
        Type MatchType { get; }
    }
}