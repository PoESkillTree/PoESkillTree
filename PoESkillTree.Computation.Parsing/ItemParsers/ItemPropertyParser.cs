using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    /// <summary>
    /// Partial parser of <see cref="ItemParser"/> that sets up properties and parses
    /// <see cref="BaseItemDefinition.Properties"/>, <see cref="Item.Quality"/>, <see cref="Item.RequiredLevel"/> and
    /// <see cref="BaseItemDefinition.Requirements"/>
    /// </summary>
    public class ItemPropertyParser : IParser<PartialItemParserParameter>
    {
        private readonly IBuilderFactories _builderFactories;
        private ModifierCollection _modifiers;

        public ItemPropertyParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        public ParseResult Parse(PartialItemParserParameter parameter)
        {
            var (item, slot, baseItemDefinition, localSource, _) = parameter;
            _modifiers = new ModifierCollection(_builderFactories, localSource);

            AddPropertySetupModifiers(slot, baseItemDefinition);
            AddPropertyModifiers(slot, baseItemDefinition);
            AddDamagePropertyModifiers(slot, baseItemDefinition);
            AddQualityModifiers(item, slot, baseItemDefinition);
            AddRequirementModifiers(item, baseItemDefinition);

            var modifiers = _modifiers.ToList();
            _modifiers = null;
            return ParseResult.Success(modifiers);
        }

        private void AddPropertySetupModifiers(ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            var tags = baseItemDefinition.Tags;
            if (tags.HasFlag(Tags.Armour))
            {
                SetupProperty(_builderFactories.StatBuilders.Armour);
                SetupProperty(_builderFactories.StatBuilders.Evasion);
                SetupProperty(_builderFactories.StatBuilders.Pool.From(Pool.EnergyShield));
            }
            if (tags.HasFlag(Tags.Shield))
            {
                SetupProperty(_builderFactories.ActionBuilders.Block.AttackChance);
            }
            if (tags.HasFlag(Tags.Weapon))
            {
                SetupDamageRelatedProperty(slot, _builderFactories.ActionBuilders.CriticalStrike.Chance);
                SetupDamageRelatedProperty(slot, _builderFactories.StatBuilders.BaseCastTime);
                SetupDamageRelatedProperty(slot, _builderFactories.StatBuilders.Range);
                foreach (var damageType in Enums.GetValues<DamageType>())
                {
                    SetupDamageRelatedProperty(slot, _builderFactories.DamageTypeBuilders.From(damageType).Damage);
                }
            }

            var requirementStats = _builderFactories.StatBuilders.Requirements;
            SetupProperty(requirementStats.Level);
            SetupProperty(requirementStats.Dexterity);
            SetupProperty(requirementStats.Intelligence);
            SetupProperty(requirementStats.Strength);
        }

        private void SetupDamageRelatedProperty(ItemSlot slot, IDamageRelatedStatBuilder stat)
            => SetupProperty(stat.WithSkills.With(SlotToHand(slot)));

        private void SetupProperty(IStatBuilder stat)
            => _modifiers.AddLocal(stat, Form.BaseSet, stat.AsItemProperty.Value);

        private void AddPropertyModifiers(ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            foreach (var (id, value) in baseItemDefinition.Properties)
            {
                switch (id)
                {
                    case "armour":
                        SetProperty(_builderFactories.StatBuilders.Armour, value);
                        break;
                    case "evasion":
                        SetProperty(_builderFactories.StatBuilders.Evasion, value);
                        break;
                    case "energy_shield":
                        SetProperty(_builderFactories.StatBuilders.Pool.From(Pool.EnergyShield), value);
                        break;
                    case "block":
                        SetProperty(_builderFactories.ActionBuilders.Block.AttackChance, value);
                        break;
                    case "critical_strike_chance":
                        SetDamageRelatedProperty(slot, _builderFactories.ActionBuilders.CriticalStrike.Chance,
                            value / 100D);
                        break;
                    case "attack_time":
                        SetDamageRelatedProperty(slot, _builderFactories.StatBuilders.BaseCastTime, value / 1000D);
                        break;
                    case "range":
                        SetDamageRelatedProperty(slot, _builderFactories.StatBuilders.Range, value);
                        break;
                }
            }
        }

        private void AddDamagePropertyModifiers(ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            int physDamageMin = 0;
            int? physDamageMax = null;
            foreach (var (id, value) in baseItemDefinition.Properties)
            {
                if (id == "physical_damage_min")
                    physDamageMin = value;
                else if (id == "physical_damage_max")
                    physDamageMax = value;
            }

            if (physDamageMax.HasValue)
            {
                var value = _builderFactories.ValueBuilders.FromMinAndMax(
                    _builderFactories.ValueBuilders.Create(physDamageMin),
                    _builderFactories.ValueBuilders.Create(physDamageMax.Value));
                var stat = _builderFactories.DamageTypeBuilders.Physical.Damage.WithSkills.With(SlotToHand(slot));
                _modifiers.AddLocal(stat.AsItemProperty, Form.BaseSet, value);
            }
        }

        private void AddQualityModifiers(Item item, ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            var value = item.Quality;
            if (value == 0)
                return;

            if (baseItemDefinition.Tags.HasFlag(Tags.Armour))
            {
                Add(_builderFactories.StatBuilders.Armour);
                Add(_builderFactories.StatBuilders.Evasion);
                Add(_builderFactories.StatBuilders.Pool.From(Pool.EnergyShield));
            }
            else if (baseItemDefinition.Tags.HasFlag(Tags.Weapon))
            {
                Add(_builderFactories.DamageTypeBuilders.Physical.Damage.WithSkills.With(SlotToHand(slot)));
            }

            void Add(IStatBuilder stat)
                => _modifiers.AddLocal(stat.AsItemProperty, Form.Increase, value);
        }

        private void AddRequirementModifiers(Item item, BaseItemDefinition baseItemDefinition)
        {
            var requirementStats = _builderFactories.StatBuilders.Requirements;
            var requirements = baseItemDefinition.Requirements;
            SetProperty(requirementStats.Level, item.RequiredLevel);
            if (requirements.Dexterity > 0)
            {
                SetProperty(requirementStats.Dexterity, requirements.Dexterity);
            }
            if (requirements.Intelligence > 0)
            {
                SetProperty(requirementStats.Intelligence, requirements.Intelligence);
            }
            if (requirements.Strength > 0)
            {
                SetProperty(requirementStats.Strength, requirements.Strength);
            }
        }

        private void SetDamageRelatedProperty(ItemSlot slot, IDamageRelatedStatBuilder stat, double value)
            => SetProperty(stat.WithSkills.With(SlotToHand(slot)), value);

        private void SetProperty(IStatBuilder stat, double value)
            => _modifiers.AddLocal(stat.AsItemProperty, Form.BaseSet, value);

        private static AttackDamageHand SlotToHand(ItemSlot slot)
            => slot == ItemSlot.MainHand ? AttackDamageHand.MainHand : AttackDamageHand.OffHand;
    }
}