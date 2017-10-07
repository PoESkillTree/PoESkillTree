using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Matching;

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


    public interface IDamageTakenConversionBuilder : IResolvable<IDamageTakenConversionBuilder>
    {
        IStatBuilder Before(IPoolStatBuilder pool);
    }
}