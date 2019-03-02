using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying pool stats.
    /// <para>These matchers are referenceable and don't reference any non-<see cref="IReferencedMatchers"/> 
    /// themselves.</para>
    /// </summary>
    public class PoolStatMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public PoolStatMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override IReadOnlyList<string> ReferenceNames { get; } =
            new[] { "StatMatchers", nameof(PoolStatMatchers) };

        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new StatMatcherCollection<IPoolStatBuilder>(_modifierBuilder)
            {
                { "(maximum )?life", Life },
                { "(maximum )?mana", Mana },
                { "(maximum )?energy shield", EnergyShield },
            };
    }
}