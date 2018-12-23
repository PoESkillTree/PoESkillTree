using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class ItemParser : IParser<ItemParserParameter>
    {
        private readonly BaseItemDefinitions _baseItemDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;
        private readonly IStatTranslator _statTranslator;

        private ModifierCollection _modifiers;

        public ItemParser(
            BaseItemDefinitions baseItemDefinitions, IBuilderFactories builderFactories, ICoreParser coreParser,
            IStatTranslator statTranslator)
            => (_coreParser, _builderFactories, _baseItemDefinitions, _statTranslator) =
                (coreParser, builderFactories, baseItemDefinitions, statTranslator);

        public ParseResult Parse(ItemParserParameter parameter)
        {
            var (item, slot) = parameter;
            _modifiers = new ModifierCollection(_builderFactories, new ModifierSource.Local.Item(slot));
            var localSource = new ModifierSource.Local.Item(slot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var baseItemDefinition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);

            AddEquipmentModifiers(item, slot, baseItemDefinition);
            AddPropertyModifiers(slot, baseItemDefinition);
            AddDamagePropertyModifiers(slot, baseItemDefinition);
            AddQualityModifiers(item, slot, baseItemDefinition);
            AddRequirementModifiers(item, baseItemDefinition);
            var parseResults = new List<ParseResult>
            {
                ParseResult.Success(_modifiers.ToList()),
                ParseBuffStats(baseItemDefinition, localSource),
            };

            var coreParseResults = item.Modifiers.Values.Flatten().Select(s => Parse(s, globalSource));
            parseResults.AddRange(coreParseResults);

            return ParseResult.Aggregate(parseResults);
        }

        private void AddEquipmentModifiers(Item item, ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            var equipmentBuilder = _builderFactories.EquipmentBuilders.Equipment[slot];
            _modifiers.AddGlobal(equipmentBuilder.ItemTags, Form.BaseSet, baseItemDefinition.Tags.EncodeAsDouble());
            _modifiers.AddGlobal(equipmentBuilder.ItemClass, Form.BaseSet, (double) baseItemDefinition.ItemClass);
            _modifiers.AddGlobal(equipmentBuilder.FrameType, Form.BaseSet, (double) item.FrameType);
            if (item.IsCorrupted)
            {
                _modifiers.AddGlobal(equipmentBuilder.Corrupted, Form.BaseSet, 1);
            }
        }

        private void AddPropertyModifiers(ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            foreach (var (id, value) in baseItemDefinition.Properties)
            {
                switch (id)
                {
                    case "armour":
                        SetProperty(slot, _builderFactories.StatBuilders.Armour, value);
                        break;
                    case "evasion":
                        SetProperty(slot, _builderFactories.StatBuilders.Evasion, value);
                        break;
                    case "energy_shield":
                        SetProperty(slot, _builderFactories.StatBuilders.Pool.From(Pool.EnergyShield), value);
                        break;
                    case "block":
                        SetProperty(slot, _builderFactories.ActionBuilders.Block.AttackChance, value);
                        break;
                    case "critical_strike_chance":
                        SetDamageRelatedProperty(slot, _builderFactories.ActionBuilders.CriticalStrike.Chance, value);
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

        private void SetDamageRelatedProperty(ItemSlot slot, IDamageRelatedStatBuilder stat, double value)
        {
            SetProperty(slot, stat.WithSkills.With(SlotToHand(slot)), value);
        }

        private void SetProperty(ItemSlot slot, IStatBuilder stat, double value)
        {
            _modifiers.AddLocal(stat.AsItemProperty(slot), Form.BaseSet, value);
            _modifiers.AddLocal(stat, Form.BaseSet, stat.AsItemProperty(slot).Value);
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
                _modifiers.AddLocal(stat.AsItemProperty(slot), Form.BaseSet, value);
                _modifiers.AddLocal(stat, Form.BaseSet, stat.AsItemProperty(slot).Value);
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
                => _modifiers.AddLocal(stat.AsItemProperty(slot), Form.Increase, value);
        }

        private void AddRequirementModifiers(Item item, BaseItemDefinition baseItemDefinition)
        {
            var requirementStats = _builderFactories.StatBuilders.Requirements;
            var requirements = baseItemDefinition.Requirements;
            _modifiers.AddLocal(requirementStats.Level, Form.BaseSet, item.RequiredLevel);
            if (requirements.Dexterity > 0)
            {
                _modifiers.AddLocal(requirementStats.Dexterity, Form.BaseSet, requirements.Dexterity);
            }
            if (requirements.Intelligence > 0)
            {
                _modifiers.AddLocal(requirementStats.Intelligence, Form.BaseSet, requirements.Intelligence);
            }
            if (requirements.Strength > 0)
            {
                _modifiers.AddLocal(requirementStats.Strength, Form.BaseSet, requirements.Strength);
            }
        }

        private ParseResult ParseBuffStats(
            BaseItemDefinition baseItemDefinition, ModifierSource.Local localSource)
        {
            if (baseItemDefinition.BuffStats.IsEmpty())
                return ParseResult.Success(new Modifier[0]);
            if (!baseItemDefinition.Tags.HasFlag(Tags.Flask))
                throw new NotSupportedException("Buff stats are only supported for flasks");

            var untranslatedStatParser = new UntranslatedStatParser(_statTranslator, _coreParser);
            var result = untranslatedStatParser.Parse(localSource, Entity.Character, baseItemDefinition.BuffStats);
            var multiplierBuilder = _builderFactories.StatBuilders.Flask.Effect.Value;
            return result.ApplyToModifiers(
                m => new Modifier(m.Stats, m.Form, ApplyMultiplier(m.Value, BuildMultiplier(m)), m.Source));

            IValue BuildMultiplier(Modifier modifier)
                => multiplierBuilder.Build(new BuildParameters(modifier.Source, Entity.Character, modifier.Form));

            IValue ApplyMultiplier(IValue value, IValue multiplier)
                => new FunctionalValue(c => value.Calculate(c) * multiplier.Calculate(c), $"{value} * {multiplier}");
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);

        private static AttackDamageHand SlotToHand(ItemSlot slot)
            => slot == ItemSlot.MainHand ? AttackDamageHand.MainHand : AttackDamageHand.OffHand;
    }

    public class ItemParserParameter
    {
        public ItemParserParameter(Item item, ItemSlot itemSlot)
            => (Item, ItemSlot) = (item, itemSlot);

        public void Deconstruct(out Item item, out ItemSlot itemSlot)
            => (item, itemSlot) = (Item, ItemSlot);

        public Item Item { get; }
        public ItemSlot ItemSlot { get; }
    }
}