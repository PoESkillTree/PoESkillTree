using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesConditionProviders : UsesStatProviders
    {
        protected UsesConditionProviders(IBuilderFactories builderFactories) 
            : base(builderFactories)
        {
        }

        protected IConditionBuilders Condition => BuilderFactories.ConditionBuilders;

        protected IConditionBuilder With(ISkillBuilderCollection skills) =>
            Condition.With(skills);

        protected IConditionBuilder With(ISkillBuilder skill) => Condition.With(skill);

        protected IConditionBuilder For(params IEntityBuilder[] targets) =>
            Condition.For(targets);

        protected IConditionBuilder And(params IConditionBuilder[] conditions) =>
            Condition.And(conditions);

        protected IConditionBuilder Or(params IConditionBuilder[] conditions) =>
            Condition.Or(conditions);

        protected IConditionBuilder Not(IConditionBuilder condition) => Condition.Not(condition);

        protected IConditionBuilder LocalIsMelee =>
            And(LocalHand.Has(Tags.Weapon), Not(LocalHand.Has(Tags.Ranged)));

        protected IConditionBuilder Unarmed => Not(MainHand.HasItem);
    }
}