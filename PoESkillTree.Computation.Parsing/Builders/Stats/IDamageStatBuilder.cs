using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IDamageStatBuilder : IStatBuilder
    {
        IStatBuilder Taken { get; }

        IDamageTakenConversionBuilder TakenFrom(IPoolStatBuilder pool);

        IConditionBuilder With();
        IConditionBuilder With(IDamageSourceBuilder source);
        IConditionBuilder With(Tags tags);
        IConditionBuilder With(IAilmentBuilder ailment);
        IConditionBuilder With(ItemSlot slot);
    }


    public interface IDamageTakenConversionBuilder
    {
        IStatBuilder Before(IPoolStatBuilder pool);
    }
}