using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Skills;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesConditionProviders : UsesStatProviders
    {
        protected UsesConditionProviders(IProviderFactories providerFactories) 
            : base(providerFactories)
        {
            Condition = providerFactories.ConditionProviderFactory;
        }

        protected IConditionProviderFactory Condition { get; }

        protected IConditionProvider With(ISkillProviderCollection skills) =>
            Condition.With(skills);

        protected IConditionProvider With(ISkillProvider skill) => Condition.With(skill);

        protected IConditionProvider For(params IEntityProvider[] targets) =>
            Condition.For(targets);

        protected IConditionProvider And(params IConditionProvider[] conditions) =>
            Condition.And(conditions);

        protected IConditionProvider Or(params IConditionProvider[] conditions) =>
            Condition.Or(conditions);

        protected IConditionProvider Not(IConditionProvider condition) => Condition.Not(condition);

        protected IConditionProvider LocalIsMelee =>
            And(LocalHand.Has(Tags.Weapon), Not(LocalHand.Has(Tags.Ranged)));

        protected IConditionProvider Unarmed => Not(MainHand.HasItem);
    }
}