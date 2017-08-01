using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Buffs;
using PoESkillTree.Computation.Providers.Charges;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Equipment;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers
{
    public interface IProviderFactories
    {
        IActionProviderFactory ActionProviderFactory { get; }

        IBuffProviderFactory BuffProviderFactory { get; }

        IChargeTypeProviderFactory ChargeTypeProviderFactory { get; }

        IConditionProviderFactory ConditionProviderFactory { get; }

        IDamageSourceProviderFactory DamageSourceProviderFactory { get; }

        IDamageTypeProviderFactory DamageTypeProviderFactory { get; }

        IEffectProviderFactory EffectProviderFactory { get; }

        IEntityProviderFactory EntityProviderFactory { get; }

        IEquipmentProviderFactory EquipmentProviderFactory { get; }

        IFormProviderFactory FormProviderFactory { get; }

        IKeywordProviderFactory KeywordProviderFactory { get; }

        ISkillProviderFactory SkillProviderFactory { get; }

        IStatProviderFactory StatProviderFactory { get; }

        IValueProviderFactory ValueProviderFactory { get; }
    }
}