using System.Collections.Generic;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillStatParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        private List<Modifier> _parsedModifiers;
        private List<UntranslatedStat> _parsedStats;
        private SkillPreParseResult _preParseResult;

        public SkillStatParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new List<Modifier>();
            _parsedStats = new List<UntranslatedStat>();
            _preParseResult = preParseResult;

            Parse(preParseResult.LevelDefinition.Stats);
            foreach (var (partIndex, additionalStats) in preParseResult.LevelDefinition.AdditionalStatsPerPart.Index())
            {
                var condition = _metaStatBuilders.MainSkillPart.Value.Eq(partIndex);
                Parse(additionalStats, condition);
            }

            var result = new PartialSkillParseResult(_parsedModifiers, _parsedStats);
            _parsedModifiers = null;
            _parsedStats = null;
            return result;
        }

        private void Parse(IReadOnlyList<UntranslatedStat> stats, IConditionBuilder partCondition = null)
        {
            ParseHitDamage(stats, partCondition);
            foreach (var stat in stats)
            {
                if (TryParseOther(stat, partCondition)
                    || TryParseDamageOverTime(stat, partCondition)
                    || TryParseConversion(stat, partCondition))
                {
                    _parsedStats.Add(stat);
                }
            }
        }

        private void ParseHitDamage(IReadOnlyList<UntranslatedStat> stats, IConditionBuilder partCondition = null)
        {
            IStatBuilder statBuilder = null;
            double hitDamageMinimum = 0D;
            double? hitDamageMaximum = null;
            foreach (var stat in stats)
            {
                var match = SkillStatIds.HitDamageRegex.Match(stat.StatId);
                if (match.Success)
                {
                    var hitDamageSource = Enums.Parse<DamageSource>(match.Groups[1].Value, true);
                    var hitDamageType = Enums.Parse<DamageType>(match.Groups[3].Value, true);
                    statBuilder = _builderFactories.DamageTypeBuilders.From(hitDamageType).Damage
                        .WithSkills(hitDamageSource);

                    if (match.Groups[2].Value == "minimum")
                        hitDamageMinimum = stat.Value;
                    else
                        hitDamageMaximum = stat.Value;

                    _parsedStats.Add(stat);
                }
            }
            if (hitDamageMaximum.HasValue)
            {
                var valueBuilder = _builderFactories.ValueBuilders.FromMinAndMax(
                    CreateValue(hitDamageMinimum), CreateValue(hitDamageMaximum.Value));
                AddMainSkillModifier(statBuilder, Form.BaseSet, valueBuilder, partCondition);
            }
        }

        private bool TryParseDamageOverTime(UntranslatedStat stat, IConditionBuilder partCondition = null)
        {
            var match = SkillStatIds.DamageOverTimeRegex.Match(stat.StatId);
            if (!match.Success)
                return false;

            var type = Enums.Parse<DamageType>(match.Groups[1].Value, true);
            var statBuilder = _builderFactories.DamageTypeBuilders.From(type).Damage
                .WithSkills(DamageSource.OverTime);
            AddMainSkillModifier(statBuilder, Form.BaseSet, stat.Value / 60D, partCondition);
            return true;
        }

        private bool TryParseConversion(UntranslatedStat stat, IConditionBuilder partCondition = null)
        {
            var match = SkillStatIds.SkillDamageConversionRegex.Match(stat.StatId);
            if (!match.Success)
                return false;

            var sourceType = Enums.Parse<DamageType>(match.Groups[1].Value, true);
            var targetType = Enums.Parse<DamageType>(match.Groups[2].Value, true);
            var sourceBuilder = _builderFactories.DamageTypeBuilders.From(sourceType).Damage.WithHitsAndAilments;
            var targetBuilder = _builderFactories.DamageTypeBuilders.From(targetType).Damage.WithHitsAndAilments;
            var conversionBuilder = sourceBuilder.ConvertTo(targetBuilder);
            AddMainSkillModifier(conversionBuilder, Form.BaseAdd, stat.Value, partCondition, isLocal: true);
            return true;
        }

        private bool TryParseOther(UntranslatedStat stat, IConditionBuilder partCondition = null)
        {
            switch (stat.StatId)
            {
                case "base_skill_number_of_additional_hits":
                    AddMainSkillModifier(_metaStatBuilders.SkillNumberOfHitsPerCast,
                        Form.BaseAdd, stat.Value, partCondition);
                    return true;
                case "skill_double_hits_when_dual_wielding":
                    AddMainSkillModifier(_metaStatBuilders.SkillDoubleHitsWhenDualWielding,
                        Form.TotalOverride, stat.Value, partCondition);
                    return true;
                case "base_use_life_in_place_of_mana":
                    ParseBloodMagic(partCondition);
                    return true;
                case "maximum_stages":
                    AddMainSkillModifier(_builderFactories.StatBuilders.SkillStage.Maximum,
                        Form.BaseSet, stat.Value, partCondition);
                    return true;
                case "hit_ailment_damage_+%_final":
                    AddMainSkillModifier(
                        _builderFactories.DamageTypeBuilders.AnyDamageType().Damage.WithHitsAndAilments,
                        Form.More, stat.Value, partCondition);
                    return true;
                case "hit_damage_+%_final":
                    AddMainSkillModifier(_builderFactories.DamageTypeBuilders.AnyDamageType().Damage.WithHits,
                        Form.More, stat.Value, partCondition);
                    return true;
                case "damage_+%_final":
                    AddMainSkillModifier(_builderFactories.DamageTypeBuilders.AnyDamageType().Damage,
                        Form.More, stat.Value, partCondition);
                    return true;
                case "cast_rate_is_melee":
                    AddMainSkillModifier(_metaStatBuilders.MainSkillPartCastRateHasKeyword(Keyword.Melee),
                        Form.TotalOverride, stat.Value, partCondition);
                    return true;
                default:
                    return false;
            }
        }

        private void ParseBloodMagic(IConditionBuilder partCondition = null)
        {
            var skillBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.MainSkillDefinition.Id);
            AddModifier(skillBuilder.ReservationPool,
                Form.TotalOverride, (double) Pool.Life,
                CombineNullableConditions(_preParseResult.IsActiveSkill, partCondition));
            var poolBuilders = _builderFactories.StatBuilders.Pool;
            AddMainSkillModifier(poolBuilders.From(Pool.Mana).Cost.ConvertTo(poolBuilders.From(Pool.Life).Cost),
                Form.BaseAdd, 100, partCondition);
        }

        private void AddMainSkillModifier(
            IStatBuilder stat, Form form, double value, IConditionBuilder condition, bool isLocal = false)
            => AddMainSkillModifier(stat, form, CreateValue(value), condition, isLocal);

        private void AddMainSkillModifier(
            IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder condition, bool isLocal = false)
            => AddModifier(stat, form, value, CombineNullableConditions(_preParseResult.IsMainSkill.IsSet, condition),
                isLocal);

        private void AddModifier(
            IStatBuilder stat, Form form, double value, IConditionBuilder condition, bool isLocal = false)
            => AddModifier(stat, form, CreateValue(value), condition, isLocal);

        private void AddModifier(
            IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder condition, bool isLocal = false)
        {
            var builder = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(value);
            if (condition != null)
                builder = builder.WithCondition(condition);
            var intermediateModifier = builder.Build();
            var modifierSource = isLocal ? (ModifierSource) _preParseResult.LocalSource : _preParseResult.GlobalSource;
            var modifiers = intermediateModifier.Build(modifierSource, Entity.Character);
            _parsedModifiers.AddRange(modifiers);
        }

        private IValueBuilder CreateValue(double value) => _builderFactories.ValueBuilders.Create(value);

        private IConditionBuilder CombineNullableConditions(IConditionBuilder left, IConditionBuilder right)
        {
            if (left is null)
                return right ?? _builderFactories.ConditionBuilders.True;
            if (right is null)
                return left;
            return left.And(right);
        }
    }
}