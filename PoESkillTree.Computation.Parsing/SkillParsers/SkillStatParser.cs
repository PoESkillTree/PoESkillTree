using System.Collections.Generic;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.Skills;

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

            if (preParseResult.HitDamageSource is DamageSource hitDamageSource)
            {
                ParseHitDamage(hitDamageSource);
            }
            foreach (var stat in preParseResult.LevelDefinition.Stats)
            {
                if (TryParseOther(stat) || TryParseDamageOverTime(stat) || TryParseConversion(stat))
                {
                    _parsedStats.Add(stat);
                }
            }

            var result = new PartialSkillParseResult(_parsedModifiers, _parsedStats);
            _parsedModifiers = null;
            _parsedStats = null;
            return result;
        }

        private void ParseHitDamage(DamageSource hitDamageSource)
        {
            DamageType? hitDamageType = null;
            double hitDamageMinimum = 0D;
            double? hitDamageMaximum = null;
            foreach (var stat in _preParseResult.LevelDefinition.Stats)
            {
                var match = SkillStatIds.HitDamageRegex.Match(stat.StatId);
                if (match.Success)
                {
                    hitDamageType = Enums.Parse<DamageType>(match.Groups[3].Value, true);
                    if (match.Groups[2].Value == "minimum")
                        hitDamageMinimum = stat.Value;
                    else
                        hitDamageMaximum = stat.Value;
                    _parsedStats.Add(stat);
                }
            }
            if (hitDamageMaximum.HasValue)
            {
                var statBuilder = _builderFactories.DamageTypeBuilders.From(hitDamageType.Value).Damage
                    .WithSkills(hitDamageSource);
                var valueBuilder = _builderFactories.ValueBuilders.FromMinAndMax(
                    CreateValue(hitDamageMinimum), CreateValue(hitDamageMaximum.Value));
                AddModifier(statBuilder, Form.BaseSet, valueBuilder);
            }
        }

        private bool TryParseDamageOverTime(UntranslatedStat stat)
        {
            var match = SkillStatIds.DamageOverTimeRegex.Match(stat.StatId);
            if (!match.Success)
                return false;

            var type = Enums.Parse<DamageType>(match.Groups[1].Value, true);
            var statBuilder = _builderFactories.DamageTypeBuilders.From(type).Damage
                .WithSkills(DamageSource.OverTime);
            AddModifier(statBuilder, Form.BaseSet, stat.Value / 60D);
            return true;
        }

        private bool TryParseConversion(UntranslatedStat stat)
        {
            var match = SkillStatIds.SkillDamageConversionRegex.Match(stat.StatId);
            if (!match.Success)
                return false;

            var sourceType = Enums.Parse<DamageType>(match.Groups[1].Value, true);
            var targetType = Enums.Parse<DamageType>(match.Groups[2].Value, true);
            var sourceBuilder = _builderFactories.DamageTypeBuilders.From(sourceType).Damage.WithHitsAndAilments;
            var targetBuilder = _builderFactories.DamageTypeBuilders.From(targetType).Damage.WithHitsAndAilments;
            var conversionBuilder = sourceBuilder.ConvertTo(targetBuilder);
            AddModifier(conversionBuilder, Form.BaseAdd, stat.Value, isLocal: true);
            return true;
        }

        private bool TryParseOther(UntranslatedStat stat)
        {
            switch (stat.StatId)
            {
                case "base_skill_number_of_additional_hits":
                    AddModifier(_metaStatBuilders.SkillNumberOfHitsPerCast, Form.BaseAdd, stat.Value);
                    return true;
                case "skill_double_hits_when_dual_wielding":
                    AddModifier(_metaStatBuilders.SkillDoubleHitsWhenDualWielding, Form.TotalOverride, stat.Value);
                    return true;
                default:
                    return false;
            }
        }

        private void AddModifier(IStatBuilder stat, Form form, double value, bool isLocal = false)
            => AddModifier(stat, form, CreateValue(value), isLocal);

        private void AddModifier(IStatBuilder stat, Form form, IValueBuilder value, bool isLocal = false)
        {
            var intermediateModifier = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(value)
                .WithCondition(_preParseResult.IsMainSkill.IsSet).Build();
            var modifierSource = isLocal ? (ModifierSource) _preParseResult.LocalSource : _preParseResult.GlobalSource;
            var modifiers = intermediateModifier.Build(modifierSource, Entity.Character);
            _parsedModifiers.AddRange(modifiers);
        }

        private IValueBuilder CreateValue(double value) => _builderFactories.ValueBuilders.Create(value);
    }
}