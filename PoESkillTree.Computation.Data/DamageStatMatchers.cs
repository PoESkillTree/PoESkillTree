using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying damage stats.
    /// <para>These matchers are referenceable and don't reference any non-<see cref="IReferencedMatchers"/> 
    /// themselves.</para>
    /// </summary>
    public class DamageStatMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public DamageStatMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override IReadOnlyList<string> ReferenceNames { get; } =
            new[] { "StatMatchers", nameof(DamageStatMatchers) };

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new StatMatcherCollection<IDamageStatBuilder>(_modifierBuilder)
            {
                // unspecific
                { "damage", Damage },
                // by source
                { "attack damage", Damage, Damage.With(Source.Attack) },
                { "spell damage", Damage, Damage.With(Source.Spell) },
                { "damage over time", Damage, Damage.With(Source.DamageOverTime) },
                // by type
                { "({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage },
                { "damage of a random element", RandomElement.Damage },
                // by source and type
                { "attack physical damage", Physical.Damage, Damage.With(Source.Attack) },
                { "physical attack damage", Physical.Damage, Damage.With(Source.Attack) },
                {
                    "({DamageTypeMatchers}) damage to attacks",
                    Reference.AsDamageType.Damage, Damage.With(Source.Attack)
                },
                {
                    "({DamageTypeMatchers}) attack damage",
                    Reference.AsDamageType.Damage, Damage.With(Source.Attack)
                },
                {
                    "({DamageTypeMatchers}) spell damage",
                    Reference.AsDamageType.Damage, Damage.With(Source.Spell)
                },
                { "burning damage", Fire.Damage, Damage.With(Source.DamageOverTime) },
                // other combinations
                { "physical melee damage", Physical.Damage, With(Skills[Keyword.Melee]) },
                { "physical weapon damage", Physical.Damage, Damage.With(Tags.Weapon) },
                {
                    "physical projectile attack damage",
                    Physical.Damage, And(Damage.With(Source.Attack), With(Skills[Keyword.Projectile]))
                },
            };
    }
}