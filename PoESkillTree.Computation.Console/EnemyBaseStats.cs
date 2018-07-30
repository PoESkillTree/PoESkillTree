using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Console
{
    /// <summary>
    /// Level 84 monster base stats as used for calculations. Only temporary until a proper model exists.
    /// </summary>
    public class EnemyBaseStats : IGivenStats
    {
        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { Entity.Enemy };

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            "+16161 to life",
            "+5389 to evasion",
            "+707 to accuracy rating",
            "+1233 to physical damage",
        };

        public IReadOnlyList<IIntermediateModifier> GivenModifiers { get; } = new IIntermediateModifier[0];
    }
}