using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    // This is a bad name
    public class ActiveSkillGeneralParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        private List<Modifier> _parsedModifiers;
        private SkillPreParseResult _preParseResult;

        public ActiveSkillGeneralParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new List<Modifier>();
            _preParseResult = preParseResult;
            var activeSkill = preParseResult.SkillDefinition.ActiveSkill;
            var hitDamageSource = preParseResult.HitDamageSource;
            var isMainSkill = preParseResult.IsMainSkill.IsSet;

            if (hitDamageSource is DamageSource s)
            {
                AddModifier(_metaStatBuilders.SkillHitDamageSource, Form.TotalOverride, (int) s);
            }
            var usesMainHandCondition = isMainSkill;
            var usesOffHandCondition = isMainSkill.And(OffHand.Has(Tags.Weapon));
            if (activeSkill.ActiveSkillTypes.Contains(ActiveSkillType.RequiresDualWield))
                usesMainHandCondition = usesMainHandCondition.And(OffHand.Has(Tags.Weapon));
            else if (activeSkill.ActiveSkillTypes.Contains(ActiveSkillType.RequiresShield))
                usesMainHandCondition = usesMainHandCondition.And(OffHand.Has(Tags.Shield));
            if (activeSkill.WeaponRestrictions.Any())
            {
                usesMainHandCondition = usesMainHandCondition.And(
                    CreateWeaponRestrictionCondition(MainHand, activeSkill.WeaponRestrictions));
                usesOffHandCondition = usesOffHandCondition.And(
                    CreateWeaponRestrictionCondition(OffHand, activeSkill.WeaponRestrictions));
            }
            AddModifier(_metaStatBuilders.SkillUsesHand(AttackDamageHand.MainHand),
                Form.TotalOverride, 1, usesMainHandCondition);
            if (!activeSkill.ActiveSkillTypes.Contains(ActiveSkillType.DoesNotUseOffHand))
            {
                AddModifier(_metaStatBuilders.SkillUsesHand(AttackDamageHand.OffHand),
                    Form.TotalOverride, 1, usesOffHandCondition);
            }
            AddModifier(_metaStatBuilders.MainSkillId,
                Form.TotalOverride, preParseResult.SkillDefinition.NumericId);

            if (hitDamageSource != DamageSource.Attack)
            {
                var castRateDamageSource = hitDamageSource ?? DamageSource.Spell;
                AddModifier(_builderFactories.StatBuilders.CastRate.With(castRateDamageSource),
                    Form.BaseSet, 1000D / activeSkill.CastTime);
            }

            if (activeSkill.TotemLifeMultiplier is double lifeMulti)
            {
                var totemLifeStat = _builderFactories.StatBuilders.Pool.From(Pool.Life)
                    .For(_builderFactories.EntityBuilders.Totem);
                AddModifier(totemLifeStat, Form.More, (lifeMulti - 1) * 100);
            }

            var result = new PartialSkillParseResult(_parsedModifiers, new UntranslatedStat[0]);
            _parsedModifiers = null;
            return result;
        }

        private void AddModifier(IStatBuilder stat, Form form, double value)
            => AddModifier(stat, form, value, _preParseResult.IsMainSkill.IsSet);

        private void AddModifier(IStatBuilder stat, Form form, double value, IConditionBuilder condition)
        {
            var intermediateModifier = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(_builderFactories.ValueBuilders.Create(value))
                .WithCondition(condition)
                .Build();
            _parsedModifiers.AddRange(intermediateModifier.Build(_preParseResult.GlobalSource, Entity.Character));
        }

        private static IConditionBuilder CreateWeaponRestrictionCondition(
            IEquipmentBuilder hand, IEnumerable<ItemClass> weaponRestrictions)
            => weaponRestrictions.Select(hand.Has).Aggregate((l, r) => l.Or(r));

        private IEquipmentBuilder MainHand => Equipment[ItemSlot.MainHand];
        private IEquipmentBuilder OffHand => Equipment[ItemSlot.OffHand];
        private IEquipmentBuilderCollection Equipment => _builderFactories.EquipmentBuilders.Equipment;
    }
}