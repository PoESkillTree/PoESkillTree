using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying converters to the modifier's stats.
    /// </summary>
    public class StatManipulatorMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public StatManipulatorMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new StatManipulatorMatcherCollection(_modifierBuilder)
            {
                { "you and nearby allies( deal| have)?", s => Buff.Aura(s, Self, Ally) },
                {
                    "auras from your skills grant (?<inner>.*) to you and allies",
                    s => Buffs(Self, Self, Ally).With(Keyword.Aura).Without(Keyword.Curse).AddStat(s), "${inner}"
                },
                {
                    "consecrated ground( you create)? grants (?<inner>.*) to you and allies",
                    s => s.For(Self).WithCondition(Ground.Consecrated.IsOn(Self))
                        .Concat(s.For(Entity.Minion).WithCondition(Ground.Consecrated.IsOn(Entity.Minion)))
                        .Concat(s.For(Entity.Totem).WithCondition(Ground.Consecrated.IsOn(Entity.Totem))),
                    "${inner}"
                },
                {
                    "consecrated ground you create applies (?<inner>.*) to enemies",
                    s => s.For(Enemy).WithCondition(Ground.Consecrated.IsOn(Enemy)), "${inner}"
                },
                {
                    "every # seconds, gain (?<inner>.*) for # seconds",
                    s => Buff.Temporary(s), "${inner}"
                },
                { "nearby enemies (have|deal)", s => Buff.Aura(s, Enemy) },
                { "nearby enemies(?= take)", s => Buff.Aura(s, Enemy) },
                { "nearby chilled enemies deal", s => Buff.Aura(s, Enemy).WithCondition(Ailment.Chill.IsOn(Enemy)) },
                { "enemies near your totems (have|deal)", s => Buff.Aura(s, Enemy).For(Entity.Totem) },
                { "enemies near your totems(?= take)", s => Buff.Aura(s, Enemy).For(Entity.Totem) },
                { "({BuffMatchers}) grants", Reference.AsBuff.AddStat },
                { "during ({SkillMatchers}) for you and allies", Reference.AsSkill.Buff.AddStat },
                { @"\(AsItemProperty\)", s => s.AsItemProperty },
                { @"\(AsPassiveNodeProperty\)", s => s.AsPassiveNodeProperty },
            };
    }
}