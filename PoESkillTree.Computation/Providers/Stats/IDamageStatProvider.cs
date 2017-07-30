using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Effects;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IDamageStatProvider : IStatProvider
    {
        IStatProvider Taken { get; }

        IDamageTakenConversionBuilder TakenFrom(IPoolStatProvider pool);

        IConditionProvider With();
        IConditionProvider With(IDamageSourceProvider source);
        IConditionProvider With(Tags tags);
        IConditionProvider With(IAilmentProvider ailment);
        IConditionProvider With(ItemSlot slot);
    }


    public interface IDamageTakenConversionBuilder
    {
        IStatProvider Before(IPoolStatProvider pool);
    }
}