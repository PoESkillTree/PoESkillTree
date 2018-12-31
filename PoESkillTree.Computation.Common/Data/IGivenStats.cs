using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.GameModel;

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
        /// The parsed modifiers that are always active.
        /// </summary>
        IReadOnlyList<IIntermediateModifier> GivenModifiers { get; }
    }
}