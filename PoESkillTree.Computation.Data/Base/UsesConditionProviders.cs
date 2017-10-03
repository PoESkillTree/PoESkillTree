using System.Linq;
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

        protected static IConditionBuilder And(IConditionBuilder condition1,
            params IConditionBuilder[] conditions) =>
            conditions.Aggregate(condition1, (l, r) => l.And(r));

        protected static IConditionBuilder Or(IConditionBuilder condition1,
            params IConditionBuilder[] conditions) =>
            conditions.Aggregate(condition1, (l, r) => l.Or(r));

        protected static IConditionBuilder Not(IConditionBuilder condition) => condition.Not;

        protected IConditionBuilder LocalIsMelee =>
            And(LocalHand.Has(Tags.Weapon), Not(LocalHand.Has(Tags.Ranged)));

        protected IConditionBuilder Unarmed => Not(MainHand.HasItem);
    }
}