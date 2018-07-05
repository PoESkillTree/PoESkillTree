using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying
    /// <see cref="Common.Builders.Actions.IActionBuilder.On"/> conditions.
    /// These must be applied after all other conditions.
    /// </summary>
    public class ActionConditionMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public ActionConditionMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new ConditionMatcherCollection(_modifierBuilder)
            {
                // generic
                { "on ({ActionMatchers})", Reference.AsAction.On },
                {
                    "when you ({ActionMatchers}) a rare or unique enemy",
                    And(Enemy.IsRareOrUnique, Reference.AsAction.On)
                },
                // kill
                { "on ({KeywordMatchers}) kill", And(With(Reference.AsKeyword), Kill.On) },
                { "when you kill an enemy,", Kill.On },
                { "if you or your totems kill an enemy", Or(Kill.On, Kill.By(Entity.Totem).On) },
                // hit
                { "from hits", Hit.On },
                { "when you are hit", Hit.By(Enemy).On },
                { "for each enemy hit by your attacks", And(With(Keyword.Attack), Hit.On) },
                // critical strike
                { "critical strikes have a", CriticalStrike.On },
                { "when you deal a critical strike", CriticalStrike.On },
                { "if you get a critical strike", CriticalStrike.On },
                // skill cast
                { "when you place a totem", Totems.Cast.On },
                // other
                { "when they block", Block.On },
                { "when you block", Block.On },
                { "when you stun an enemy", Action.Stun.On },
                { "after spending # mana", Action.SpendMana(Value).On },
                { "when you gain a ({ChargeTypeMatchers})", Reference.AsChargeType.GainAction.On },
                { "you gain", Condition.True }, // may be left over at the end, does nothing
                // unique
                {
                    "when your trap is triggered by an enemy",
                    Action.Unique("When your Trap is triggered by an Enemy").On
                },
                {
                    "when your mine is detonated targeting an enemy",
                    Action.Unique("When your Mine is detonated targeting an Enemy").On
                },
            };
    }
}