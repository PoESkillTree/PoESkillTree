using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers
{
    public interface IEquipmentProvider
    {
        IConditionProvider Has(Tags tag);
    }

    public static class EquipmentProviders
    {
        public static readonly IEquipmentProvider MainHand;
        public static readonly IEquipmentProvider OffHand;
    }
}