using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    /// <summary>
    /// Collection of the stats that are always applied (to the entity the collection belongs to)
    /// </summary>
    public interface IGivenStats
    {
        /// <summary>
        /// The unparsed stat lines that are always active.
        /// </summary>
        IReadOnlyList<string> GivenStatLines { get; }

        /// <summary>
        /// The parsed stats that are always active.
        /// </summary>
        IReadOnlyList<GivenStatData> GivenStats { get; }
    }
}