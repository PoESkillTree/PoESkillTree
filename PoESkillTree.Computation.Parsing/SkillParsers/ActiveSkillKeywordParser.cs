using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillKeywordParser : IPartialSkillParser
    {
        private static readonly IReadOnlyList<Keyword> KeywordsExcludedForDamageOverTime = new[]
        {
            Keyword.Attack, Keyword.Spell, Keyword.Melee, Keyword.Projectile, Keyword.AreaOfEffect, Keyword.Movement,
            Keyword.Bow,
        };

        private static readonly IReadOnlyList<string> AreaDamageOverTimeSkills = new[]
        {
            "PoisonArrow", "ColdSnap", "VaalColdSnap", "Desecrate", "FireTrap", "RighteousFire", "VaalRighteousFire",
            "FrostBoltNova"
        };

        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        private List<Modifier> _parsedModifiers;
        private List<UntranslatedStat> _parsedStats;
        private SkillDefinition _skillDefinition;
        private SkillPreParseResult _preParseResult;

        public ActiveSkillKeywordParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill skill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new List<Modifier>();
            _parsedStats = new List<UntranslatedStat>();
            _preParseResult = preParseResult;
            _skillDefinition = preParseResult.SkillDefinition;

            var hitDamageSource = preParseResult.HitDamageSource;
            var isMainSkill = preParseResult.IsMainSkill.IsSet;

            AddKeywordModifiers(_metaStatBuilders.MainSkillHasKeyword, _ => isMainSkill);
            AddKeywordModifiers(
                _metaStatBuilders.MainSkillPartHasKeyword,
                k => PartHasKeywordCondition(hitDamageSource, isMainSkill, k));
            AddKeywordModifiers(
                _metaStatBuilders.MainSkillPartCastRateHasKeyword,
                k => PartHasKeywordCondition(hitDamageSource, isMainSkill, k));
            if (hitDamageSource.HasValue)
            {
                var hitIsAreaDamage = HitDamageIsArea(preParseResult.LevelDefinition);
                AddKeywordModifiers(
                    k => _metaStatBuilders.MainSkillPartDamageHasKeyword(k, hitDamageSource.Value),
                    k => PartHasKeywordCondition(hitDamageSource, isMainSkill, k),
                    k => k != Keyword.AreaOfEffect || hitIsAreaDamage);
            }
            if (preParseResult.HasSkillDamageOverTime)
            {
                var dotIsAreaDamage = AreaDamageOverTimeSkills.Contains(_skillDefinition.Id);
                AddKeywordModifiers(
                    k => _metaStatBuilders.MainSkillPartDamageHasKeyword(k, DamageSource.OverTime),
                    k => PartHasKeywordCondition(hitDamageSource, isMainSkill, k),
                    k => k == Keyword.AreaOfEffect ? dotIsAreaDamage : !KeywordsExcludedForDamageOverTime.Contains(k));
            }
            AddKeywordModifiers(
                k => _metaStatBuilders.MainSkillPartAilmentDamageHasKeyword(k),
                k => PartHasKeywordCondition(hitDamageSource, isMainSkill, k),
                k => !KeywordsExcludedForDamageOverTime.Contains(k));

            var result = new PartialSkillParseResult(_parsedModifiers, _parsedStats);
            _parsedModifiers = null;
            _parsedStats = null;
            return result;
        }

        private void AddKeywordModifiers(
            Func<Keyword, IStatBuilder> statFactory,
            Func<Keyword, IConditionBuilder> conditionFactory,
            Func<Keyword, bool> preCondition = null)
        {
            var modifiers =
                KeywordModifiers(_skillDefinition.ActiveSkill.Keywords, statFactory, conditionFactory, preCondition)
                    .SelectMany(m => m.Build(_preParseResult.GlobalSource, Entity.Character));
            _parsedModifiers.AddRange(modifiers);
        }

        private IEnumerable<IIntermediateModifier> KeywordModifiers(
            IEnumerable<Keyword> keywords, Func<Keyword, IStatBuilder> statFactory,
            Func<Keyword, IConditionBuilder> conditionFactory, Func<Keyword, bool> preCondition = null)
        {
            if (preCondition != null)
                keywords = keywords.Where(preCondition);
            foreach (var keyword in keywords)
            {
                yield return _modifierBuilder
                    .WithStat(statFactory(keyword))
                    .WithForm(_builderFactories.FormBuilders.TotalOverride)
                    .WithValue(_builderFactories.ValueBuilders.Create(1))
                    .WithCondition(conditionFactory(keyword)).Build();
            }
        }

        private bool HitDamageIsArea(SkillLevelDefinition level)
        {
            var firstMatch = level.Stats.Where(s => s.StatId == SkillStatIds.IsAreaDamage)
                .Cast<UntranslatedStat?>()
                .FirstOrDefault();
            if (firstMatch is UntranslatedStat stat)
            {
                _parsedStats.Add(stat);
                return true;
            }
            return false;
        }

        private IConditionBuilder PartHasKeywordCondition(
            DamageSource? hitDamageSource, IConditionBuilder baseCondition, Keyword keyword)
        {
            var mainHandIsRanged = _builderFactories.EquipmentBuilders.Equipment[ItemSlot.MainHand].Has(Tags.Ranged);
            switch (keyword)
            {
                case Keyword.Melee:
                    return baseCondition.And(mainHandIsRanged.Not);
                case Keyword.Projectile when hitDamageSource == DamageSource.Attack:
                    return baseCondition.And(mainHandIsRanged);
                default:
                    return baseCondition;
            }
        }
    }
}