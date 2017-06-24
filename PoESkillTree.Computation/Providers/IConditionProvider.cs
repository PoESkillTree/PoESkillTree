using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IConditionProvider
    {

    }

    public static class ConditionProviders
    {
        public static readonly IConditionProvider Hit;
        public static readonly IConditionProvider WeaponLocalHit;

        public static IConditionProvider DamageCondition(IKeywordProvider hasKeyword, bool? isUnarmed = null)
        {
            throw new NotImplementedException();
        }
        public static IConditionProvider And(params IConditionProvider[] conditions)
        {
            throw new NotImplementedException();
        }
    }
}