using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Charges;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Equipment;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Matching
{
    public interface IGroupConverter
    {
        T As<T>() where T : IStatProvider;

        IDamageTypeProvider AsDamageType { get; }

        IChargeTypeProvider AsChargeType { get; }

        IAilmentProvider AsAilment { get; }

        IKeywordProvider AsKeyword { get; }

        IItemSlotProvider AsItemSlot { get; }

        IActionProvider<ISelfProvider, IEntityProvider> AsAction { get; }

        IStatProvider AsStat { get; }

        ISkillProvider AsSkill { get; }
    }
}