using System.Collections.Generic;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
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

            ParseHitDamage();
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

        private void ParseHitDamage()
        {
            IStatBuilder statBuilder = null;
            double hitDamageMinimum = 0D;
            double? hitDamageMaximum = null;
            foreach (var stat in _preParseResult.LevelDefinition.Stats)
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
                AddMainSkillModifier(statBuilder, Form.BaseSet, valueBuilder);
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
            AddMainSkillModifier(statBuilder, Form.BaseSet, stat.Value / 60D);
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
            AddMainSkillModifier(conversionBuilder, Form.BaseAdd, stat.Value, isLocal: true);
            return true;
        }

        private bool TryParseOther(UntranslatedStat stat)
        {
            switch (stat.StatId)
            {
                case "base_skill_number_of_additional_hits":
                    AddMainSkillModifier(_metaStatBuilders.SkillNumberOfHitsPerCast, Form.BaseAdd, stat.Value);
                    return true;
                case "skill_double_hits_when_dual_wielding":
                    AddMainSkillModifier(_metaStatBuilders.SkillDoubleHitsWhenDualWielding,
                        Form.TotalOverride, stat.Value);
                    return true;
                case "base_use_life_in_place_of_mana":
                    ParseBloodMagic();
                    return true;
                default:
                    return false;
            }
        }

        private void ParseBloodMagic()
        {
            var skillBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.MainSkillDefinition.Id);
            AddModifier(skillBuilder.ReservationPool,
                Form.TotalOverride, (double) Pool.Life, _preParseResult.IsActiveSkill);
            var poolBuilders = _builderFactories.StatBuilders.Pool;
            AddMainSkillModifier(poolBuilders.From(Pool.Mana).Cost.ConvertTo(poolBuilders.From(Pool.Life).Cost),
                Form.BaseAdd, 100);
        }

        private void AddMainSkillModifier(IStatBuilder stat, Form form, double value, bool isLocal = false)
            => AddMainSkillModifier(stat, form, CreateValue(value), isLocal);

        private void AddMainSkillModifier(IStatBuilder stat, Form form, IValueBuilder value, bool isLocal = false)
            => AddModifier(stat, form, value, _preParseResult.IsMainSkill.IsSet, isLocal);

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
    }
}