using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data
{
    public class DamageStatMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;

        public DamageStatMatchers(IBuilderFactories builderFactories, 
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override IReadOnlyList<string> ReferenceNames { get; } =
            new[] { "StatMatchers", nameof(DamageStatMatchers) };

        public bool MatchesWholeLineOnly => false;

        public IEnumerator<MatcherData> GetEnumerator() =>
            new StatMatcherCollection<IDamageStatBuilder>(_modifierBuilder, ValueFactory)
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
                { "claw physical damage", Physical.Damage, Damage.With(Tags.Claw) },
                { "physical weapon damage", Physical.Damage, Damage.With(Tags.Weapon) },
                {
                    "physical projectile attack damage",
                    Physical.Damage,
                    And(Damage.With(Source.Attack), With(Skills[Keyword.Projectile]))
                },
            }.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}