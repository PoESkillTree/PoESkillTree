using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    public static class StatBuilderExtensions
    {
        /// <summary>
        /// Builds this instance and selects the stats from the results.
        /// </summary>
        public static IEnumerable<IStat> BuildToStats(this IStatBuilder @this, Entity modifierSourceEntity)
            => @this.Build(new BuildParameters(new ModifierSource.Global(), modifierSourceEntity, default))
                .SelectMany(r => r.Stats);
    }
}