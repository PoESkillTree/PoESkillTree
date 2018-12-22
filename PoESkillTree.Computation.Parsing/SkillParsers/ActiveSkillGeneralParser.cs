using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    // This is a bad name
    public class ActiveSkillGeneralParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;

        private SkillModifierCollection _parsedModifiers;

        public ActiveSkillGeneralParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new SkillModifierCollection(_builderFactories,
                preParseResult.IsMainSkill, preParseResult.LocalSource);
            var activeSkill = preParseResult.SkillDefinition.ActiveSkill;
            var isMainSkill = preParseResult.IsMainSkill;
            var isActiveSkill = preParseResult.IsActiveSkill;

            AddHitDamageSourceModifiers(preParseResult);

            var offHandHasWeapon = OffHand.Has(Tags.Weapon);
            var usesMainHandCondition = isMainSkill;
            var usesOffHandCondition = isMainSkill.And(offHandHasWeapon);
            if (activeSkill.ActiveSkillTypes.Contains(ActiveSkillType.RequiresDualWield))
                usesMainHandCondition = usesMainHandCondition.And(offHandHasWeapon);
            else if (activeSkill.ActiveSkillTypes.Contains(ActiveSkillType.RequiresShield))
                usesMainHandCondition = usesMainHandCondition.And(OffHand.Has(Tags.Shield));
            if (activeSkill.WeaponRestrictions.Any())
            {
                var suitableMainHand = CreateWeaponRestrictionCondition(MainHand, activeSkill.WeaponRestrictions);
                var suitableOffHand = CreateWeaponRestrictionCondition(OffHand, activeSkill.WeaponRestrictions);
                usesMainHandCondition = usesMainHandCondition
                    .And(suitableMainHand).And(suitableOffHand.Or(offHandHasWeapon.Not));
                usesOffHandCondition = usesOffHandCondition
                    .And(suitableMainHand).And(suitableOffHand);
            }
            _parsedModifiers.AddGlobal(_metaStatBuilders.SkillUsesHand(AttackDamageHand.MainHand),
                Form.TotalOverride, 1, usesMainHandCondition);
            _parsedModifiers.AddGlobal(_metaStatBuilders.SkillUsesHand(AttackDamageHand.OffHand),
                Form.TotalOverride, 1, usesOffHandCondition);

            _parsedModifiers.AddGlobalForMainSkill(_metaStatBuilders.MainSkillId,
                Form.TotalOverride, preParseResult.SkillDefinition.NumericId);

            _parsedModifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.BaseCastTime.With(DamageSource.Spell),
                Form.BaseSet, activeSkill.CastTime / 1000D);
            _parsedModifiers.AddGlobalForMainSkill(
                _builderFactories.StatBuilders.BaseCastTime.With(DamageSource.Secondary),
                Form.BaseSet, activeSkill.CastTime / 1000D);

            if (activeSkill.TotemLifeMultiplier is double lifeMulti)
            {
                var totemLifeStat = _builderFactories.StatBuilders.Pool.From(Pool.Life)
                    .For(_builderFactories.EntityBuilders.Totem);
                _parsedModifiers.AddGlobalForMainSkill(totemLifeStat, Form.More, (lifeMulti - 1) * 100);
            }

            _parsedModifiers.AddGlobal(_metaStatBuilders.ActiveSkillItemSlot(mainSkill.Id),
                Form.BaseSet, (double) mainSkill.ItemSlot);
            _parsedModifiers.AddGlobal(_metaStatBuilders.ActiveSkillSocketIndex(mainSkill.Id),
                Form.BaseSet, mainSkill.SocketIndex);

            if (activeSkill.ProvidesBuff)
            {
                var allBuffStats =
                    preParseResult.LevelDefinition.BuffStats.Concat(preParseResult.LevelDefinition.QualityBuffStats);
                var allAffectedEntities = allBuffStats.SelectMany(s => s.AffectedEntities).Distinct().ToList();
                if (allAffectedEntities.Any())
                {
                    var target = _builderFactories.EntityBuilders.From(allAffectedEntities);
                    _parsedModifiers.AddGlobal(_builderFactories.SkillBuilders.FromId(mainSkill.Id).Buff.On(target),
                        Form.BaseSet, 1, isActiveSkill);
                }
            }

            _parsedModifiers.AddGlobal(_builderFactories.SkillBuilders.FromId(mainSkill.Id).Instances,
                Form.BaseAdd, 1, isActiveSkill);
            _parsedModifiers.AddGlobal(_builderFactories.SkillBuilders.AllSkills.CombinedInstances,
                Form.BaseAdd, 1, isActiveSkill);
            foreach (var keyword in activeSkill.Keywords)
            {
                var keywordBuilder = _builderFactories.KeywordBuilders.From(keyword);
                _parsedModifiers.AddGlobal(_builderFactories.SkillBuilders[keywordBuilder].CombinedInstances,
                    Form.BaseAdd, 1, isActiveSkill);
            }

            var result = new PartialSkillParseResult(_parsedModifiers, new UntranslatedStat[0]);
            _parsedModifiers = null;
            return result;
        }

        private void AddHitDamageSourceModifiers(SkillPreParseResult preParseResult)
        {
            var partCount = preParseResult.LevelDefinition.AdditionalStatsPerPart.Count;

            for (var partIndex = 0; partIndex < partCount; partIndex++)
            {
                if (DetermineHitDamageSource(preParseResult, partIndex) is DamageSource damageSource)
                {
                    var condition =
                        partCount > 1 ? _builderFactories.StatBuilders.MainSkillPart.Value.Eq(partIndex) : null;
                    _parsedModifiers.AddGlobalForMainSkill(_metaStatBuilders.SkillHitDamageSource,
                        Form.TotalOverride, (int) damageSource, condition);
                }
            }
        }

        private static DamageSource? DetermineHitDamageSource(SkillPreParseResult preParseResult, int partIndex)
        {
            var statIds = preParseResult.LevelDefinition.Stats
                .Concat(preParseResult.LevelDefinition.AdditionalStatsPerPart[partIndex])
                .Select(s => s.StatId).ToList();

            if (statIds.Any(s => s == SkillStatIds.DealsSecondaryDamage))
            {
                return DamageSource.Secondary;
            }

            if (preParseResult.SkillDefinition.ActiveSkill.ActiveSkillTypes.Contains(ActiveSkillType.Attack))
            {
                return DamageSource.Attack;
            }

            foreach (var statId in statIds)
            {
                var match = SkillStatIds.HitDamageRegex.Match(statId);
                if (match.Success)
                    return Enums.Parse<DamageSource>(match.Groups[1].Value, true);
            }
            return null;
        }

        private static IConditionBuilder CreateWeaponRestrictionCondition(
            IEquipmentBuilder hand, IEnumerable<ItemClass> weaponRestrictions)
            => weaponRestrictions.Select(hand.Has).Aggregate((l, r) => l.Or(r));

        private IEquipmentBuilder MainHand => Equipment[ItemSlot.MainHand];
        private IEquipmentBuilder OffHand => Equipment[ItemSlot.OffHand];
        private IEquipmentBuilderCollection Equipment => _builderFactories.EquipmentBuilders.Equipment;
    }
}