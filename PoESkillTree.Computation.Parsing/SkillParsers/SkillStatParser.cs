using System.Collections.Generic;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
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

        private SkillModifierCollection _parsedModifiers;
        private List<UntranslatedStat> _parsedStats;
        private SkillPreParseResult _preParseResult;

        public SkillStatParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new SkillModifierCollection(_builderFactories,
                preParseResult.IsMainSkill, preParseResult.LocalSource);
            _parsedStats = new List<UntranslatedStat>();
            _preParseResult = preParseResult;

            Parse(preParseResult.LevelDefinition.Stats);
            foreach (var (partIndex, additionalStats) in preParseResult.LevelDefinition.AdditionalStatsPerPart.Index())
            {
                var condition = _builderFactories.StatBuilders.MainSkillPart.Value.Eq(partIndex);
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
                _parsedModifiers.AddGlobalForMainSkill(statBuilder, Form.BaseSet, valueBuilder, partCondition);
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
            _parsedModifiers.AddGlobalForMainSkill(statBuilder, Form.BaseSet, stat.Value / 60D, partCondition);
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
            _parsedModifiers.AddLocalForMainSkill(conversionBuilder, Form.BaseAdd, stat.Value, partCondition);
            return true;
        }

        private bool TryParseOther(UntranslatedStat stat, IConditionBuilder partCondition = null)
        {
            switch (stat.StatId)
            {
                case "base_skill_number_of_additional_hits":
                    _parsedModifiers.AddGlobalForMainSkill(_metaStatBuilders.SkillNumberOfHitsPerCast,
                        Form.BaseAdd, stat.Value, partCondition);
                    return true;
                case "skill_double_hits_when_dual_wielding":
                    _parsedModifiers.AddGlobalForMainSkill(_metaStatBuilders.SkillDoubleHitsWhenDualWielding,
                        Form.TotalOverride, stat.Value, partCondition);
                    return true;
                case "base_use_life_in_place_of_mana":
                    ParseBloodMagic(partCondition);
                    return true;
                case "maximum_stages":
                    _parsedModifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.SkillStage.Maximum,
                        Form.BaseSet, stat.Value, partCondition);
                    return true;
                case "cast_rate_is_melee":
                    _parsedModifiers.AddGlobalForMainSkill(
                        _metaStatBuilders.MainSkillPartCastRateHasKeyword(Keyword.Melee),
                        Form.TotalOverride, stat.Value, partCondition);
                    return true;
                case "hit_rate_ms":
                    _parsedModifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.HitRate,
                        Form.BaseSet, 1000D / stat.Value, partCondition);
                    return true;
                default:
                    return false;
            }
        }

        private void ParseBloodMagic(IConditionBuilder partCondition = null)
        {
            var skillBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.MainSkillDefinition.Id);
            _parsedModifiers.AddGlobal(skillBuilder.ReservationPool,
                Form.TotalOverride, (double) Pool.Life,
                _preParseResult.IsActiveSkill.And(partCondition ?? _builderFactories.ConditionBuilders.True));
            var poolBuilders = _builderFactories.StatBuilders.Pool;
            _parsedModifiers.AddGlobalForMainSkill(
                poolBuilders.From(Pool.Mana).Cost.ConvertTo(poolBuilders.From(Pool.Life).Cost),
                Form.BaseAdd, 100, partCondition);
        }

        private IValueBuilder CreateValue(double value) => _builderFactories.ValueBuilders.Create(value);
    }
}