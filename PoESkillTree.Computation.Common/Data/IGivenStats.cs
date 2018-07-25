using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Collection of stats that are always applied
    /// </summary>
    public interface IGivenStats
    {
        /// <summary>
        /// The entities these stats are applied to
        /// </summary>
        IReadOnlyList<Entity> AffectedEntities { get; }

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