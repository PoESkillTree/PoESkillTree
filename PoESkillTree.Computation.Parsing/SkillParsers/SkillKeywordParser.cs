using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillKeywordParser : IPartialSkillParser
    {
        private static readonly IReadOnlyDictionary<DamageSource, IReadOnlyList<Keyword>> ExcludedKeywords =
            new Dictionary<DamageSource, IReadOnlyList<Keyword>>
            {
                [DamageSource.Attack] = new[] { Keyword.Spell, },
                [DamageSource.Spell] = new[] { Keyword.Attack, Keyword.Melee, Keyword.Bow, },
                [DamageSource.Secondary] = new[] { Keyword.Attack, Keyword.Spell, Keyword.Melee, Keyword.Bow, },
                [DamageSource.OverTime] = new[]
                {
                    Keyword.Attack, Keyword.Spell, Keyword.Melee, Keyword.Projectile, Keyword.Movement, Keyword.Bow,
                },
            };

        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly ISkillKeywordSelector _keywordSelector;

        private ModifierCollection _parsedModifiers;
        private ISet<UntranslatedStat> _parsedStats;
        private SkillPreParseResult _preParseResult;

        private SkillKeywordParser(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            ISkillKeywordSelector keywordSelector)
            => (_builderFactories, _metaStatBuilders, _keywordSelector) =
                (builderFactories, metaStatBuilders, keywordSelector);

        public static IPartialSkillParser CreateActive(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => new SkillKeywordParser(builderFactories, metaStatBuilders, new ActiveSkillKeywordSelector());

        public static IPartialSkillParser CreateSupport(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => new SkillKeywordParser(builderFactories, metaStatBuilders, new SupportSkillKeywordSelector());

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new ModifierCollection(_builderFactories, preParseResult.LocalSource);
            _parsedStats = new HashSet<UntranslatedStat>();
            _preParseResult = preParseResult;

            var isMainSkill = preParseResult.IsMainSkill.IsSet;
            var alwaysProjectileParts = GetPartsWithFlagStat(SkillStatIds.IsAlwaysProjectile);
            var areaDamageHitParts = GetPartsWithFlagStat(SkillStatIds.IsAreaDamage);
            var areaDamageDotParts = GetPartsWithFlagStat(SkillStatIds.SkillDotIsAreaDamage);

            IConditionBuilder CreatePartHasKeywordCondition(Keyword k, int partIndex)
                => PartHasKeywordCondition(alwaysProjectileParts.Contains(partIndex), isMainSkill, k);

            AddKeywordModifiers(_metaStatBuilders.MainSkillHasKeyword, isMainSkill);
            AddPartKeywordModifiers(_metaStatBuilders.MainSkillPartHasKeyword, CreatePartHasKeywordCondition);
            AddPartKeywordModifiers(_metaStatBuilders.MainSkillPartCastRateHasKeyword, CreatePartHasKeywordCondition);
            foreach (var damageSource in Enums.GetValues<DamageSource>())
            {
                var preCondition = damageSource != DamageSource.OverTime
                    ? KeywordPreCondition(areaDamageHitParts, ExcludedKeywords[damageSource])
                    : KeywordPreCondition(areaDamageDotParts, ExcludedKeywords[damageSource]);
                AddPartKeywordModifiers(k => _metaStatBuilders.MainSkillPartDamageHasKeyword(k, damageSource),
                    CreatePartHasKeywordCondition, preCondition);
            }
            AddPartKeywordModifiers(k => _metaStatBuilders.MainSkillPartAilmentDamageHasKeyword(k),
                CreatePartHasKeywordCondition,
                KeywordPreCondition(new int[0], ExcludedKeywords[DamageSource.OverTime]));

            var result = new PartialSkillParseResult(_parsedModifiers, _parsedStats);
            _parsedModifiers = null;
            _parsedStats = null;
            return result;
        }

        private void AddKeywordModifiers(Func<Keyword, IStatBuilder> statFactory, IConditionBuilder condition)
        {
            var keywords = _keywordSelector.GetKeywords(_preParseResult.SkillDefinition);
            AddKeywordModifiers(keywords, statFactory, _ => condition);
        }

        private void AddPartKeywordModifiers(
            Func<Keyword, IStatBuilder> statFactory,
            Func<Keyword, int, IConditionBuilder> conditionFactory,
            Func<Keyword, int, bool> preCondition = null)
        {
            if (preCondition is null)
                preCondition = (_, __) => true;
            var keywordsPerPart = _keywordSelector.GetKeywordsPerPart(_preParseResult.SkillDefinition);
            if (keywordsPerPart.Count == 1)
            {
                AddKeywordModifiers(keywordsPerPart[0], statFactory,
                    k => conditionFactory(k, 0), k => preCondition(k, 0));
                return;
            }
            foreach (var (partIndex, keywords) in keywordsPerPart.Index())
            {
                var isSkillPart = _builderFactories.StatBuilders.MainSkillPart.Value.Eq(partIndex);
                AddKeywordModifiers(keywords, statFactory,
                    k => conditionFactory(k, partIndex).And(isSkillPart), k => preCondition(k, partIndex));
            }
        }

        private void AddKeywordModifiers(
            IEnumerable<Keyword> keywords, Func<Keyword, IStatBuilder> statFactory,
            Func<Keyword, IConditionBuilder> conditionFactory, Func<Keyword, bool> preCondition = null)
        {
            if (preCondition != null)
                keywords = keywords.Where(preCondition);
            foreach (var keyword in keywords)
            {
                _parsedModifiers.AddGlobal(statFactory(keyword), Form.TotalOverride, 1, conditionFactory(keyword));
            }
        }

        private ISet<int> GetPartsWithFlagStat(string statId)
        {
            var level = _preParseResult.LevelDefinition;
            if (MatchFlagStat(level.Stats, statId))
                return Enumerable.Range(0, level.AdditionalStatsPerPart.Count).ToHashSet();

            var partIndices = new HashSet<int>();
            foreach (var (partIndex, stats) in level.AdditionalStatsPerPart.Index())
            {
                if (MatchFlagStat(stats, statId))
                    partIndices.Add(partIndex);
            }
            return partIndices;
        }

        private bool MatchFlagStat(IEnumerable<UntranslatedStat> stats, string statId)
        {
            var firstMatch = stats.FirstOrDefault(s => s.StatId == statId);
            if (firstMatch is UntranslatedStat stat)
            {
                _parsedStats.Add(stat);
                return true;
            }
            return false;
        }

        private IConditionBuilder PartHasKeywordCondition(
            bool isAlwaysProjectile, IConditionBuilder baseCondition, Keyword keyword)
        {
            var mainHandIsRanged = _builderFactories.EquipmentBuilders.Equipment[ItemSlot.MainHand].Has(Tags.Ranged);
            switch (keyword)
            {
                case Keyword.Melee:
                    return baseCondition.And(mainHandIsRanged.Not);
                case Keyword.Projectile when !isAlwaysProjectile:
                    return baseCondition.And(mainHandIsRanged);
                default:
                    return baseCondition;
            }
        }

        private static Func<Keyword, int, bool> KeywordPreCondition(
            IEnumerable<int> areaDamageParts, IEnumerable<Keyword> excludedKeywords)
        {
            return (k, partIndex) => k == Keyword.AreaOfEffect
                ? areaDamageParts.Contains(partIndex)
                : !excludedKeywords.Contains(k);
        }
    }
}