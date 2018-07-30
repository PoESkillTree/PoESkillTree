using System.Collections.Generic;
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

        public override IReadOnlyList<string> ReferenceNames { get; } = new[] { "StatMatchers" };

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new StatMatcherCollection<IDamageRelatedStatBuilder>(_modifierBuilder)
            {
                // unspecific
                { "damage", Damage },
                // by source
                { "attack damage", Damage.WithSkills(DamageSource.Attack) },
                { "spell damage", Damage.WithSkills(DamageSource.Spell) },
                { "damage over time", Damage.With(DamageSource.OverTime) },
                // by type
                { "({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage },
                { "damage of a random element", RandomElement.Damage },
                // by skill vs. ailment
                { "damage with hits and ailments", Damage.WithHitsAndAilments },
                { "(?<!no )damage (with|from) hits", Damage.WithHits },
                { "damage with ailments", Damage.WithAilments },
                { "damage with ({AilmentMatchers})", Damage.With(Reference.AsAilment) },
                // by source and type
                { "attack physical damage", Physical.Damage.WithSkills(DamageSource.Attack) },
                {
                    "({DamageTypeMatchers}) damage to attacks",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Attack)
                },
                {
                    "({DamageTypeMatchers}) attack damage",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Attack)
                },
                {
                    "({DamageTypeMatchers}) spell damage",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Spell)
                },
                { "burning damage", Fire.Damage.WithSkills(DamageSource.OverTime), Fire.Damage.With(Ailment.Ignite) },
                // other combinations
                { "(?<!no )({DamageTypeMatchers}) damage (with|from) hits", Reference.AsDamageType.Damage.WithHits },
                // specific attack damage
                { "melee damage", Damage.WithSkills(DamageSource.Attack), With(Keyword.Melee) },
                { "melee physical damage", Physical.Damage.WithSkills(DamageSource.Attack), With(Keyword.Melee) },
                { "physical melee damage", Physical.Damage.WithSkills(DamageSource.Attack), With(Keyword.Melee) },
                { "physical weapon damage", Physical.Damage.WithSkills(DamageSource.Attack), MainHand.HasItem },
                {
                    "physical projectile attack damage",
                    Physical.Damage.WithSkills(DamageSource.Attack), With(Keyword.Projectile)
                },
            }; //add
    }
}