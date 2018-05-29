using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
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
            new StatMatcherCollection<IDamageRelatedStatBuilder>(_modifierBuilder)
            {
                // unspecific
                { "damage", Damage },
                // by source
                { "attack damage", Damage.With(Source.Attack()) },
                { "spell damage", Damage.With(Source.Spell()) },
                { "damage over time", Damage.With(Source.OverTime()) },
                // by type
                { "({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage },
                { "damage of a random element", RandomElement.Damage },
                // by skill vs. ailment
                { "damage with hits and ailments", Damage.WithHitsAndAilments },
                { "(?<!no )damage (with|from) hits", Damage.WithHits },
                { "damage with ailments", Damage.WithAilments },
                { "damage with ({AilmentMatchers})", Damage.With(Reference.AsAilment) },
                // by source and type
                { "attack physical damage", Physical.Damage.With(Source.Attack()) },
                {
                    "({DamageTypeMatchers}) damage to attacks",
                    Reference.AsDamageType.Damage.With(Source.Attack())
                },
                {
                    "({DamageTypeMatchers}) attack damage",
                    Reference.AsDamageType.Damage.With(Source.Attack())
                },
                {
                    "({DamageTypeMatchers}) spell damage",
                    Reference.AsDamageType.Damage.With(Source.Spell())
                },
                { "burning damage", Fire.Damage.With(Source.OverTime()) },
                // other combinations
                { "(?<!no )({DamageTypeMatchers}) damage (with|from) hits", Reference.AsDamageType.Damage.WithHits },
                { "physical melee damage", Physical.Damage, With(Keyword.Melee) },
                { "physical weapon damage", Physical.Damage.With(Source.Attack()), MainHand.HasItem },
                {
                    "physical projectile attack damage",
                    Physical.Damage.With(Source.Attack()), With(Keyword.Projectile)
                },
            }; //add
    }
}