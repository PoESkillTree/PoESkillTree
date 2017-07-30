using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Equipment
{
    public interface IEquipmentProvider
    {
        IConditionProvider Has(Tags tag);
        IConditionProvider Has(FrameType frameType);
        IConditionProvider HasItem { get; }
        IConditionProvider IsCorrupted { get; }

        IFlagStatProvider AppliesToSelf { get; } // default: 1
        IFlagStatProvider AppliesToMinions { get; }
    }
}