using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Equipment
{
    public interface IEquipmentBuilder : IResolvable<IEquipmentBuilder>
    {
        IConditionBuilder Has(Tags tag);
        IConditionBuilder Has(FrameType frameType);
        IConditionBuilder HasItem { get; }
        IConditionBuilder IsCorrupted { get; }

        IFlagStatBuilder AppliesToSelf { get; } // default: 1
        IFlagStatBuilder AppliesToMinions { get; }
    }
}