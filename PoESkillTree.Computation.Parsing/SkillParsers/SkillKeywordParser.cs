using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

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

        private static readonly IReadOnlyList<string> AreaDamageOverTimeSkills = new[]
        {
            "PoisonArrow", "ColdSnap", "VaalColdSnap", "Desecrate", "FireTrap", "RighteousFire", "VaalRighteousFire",
            "FrostBoltNova"
        };

        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();
        private readonly Func<SkillDefinition, IEnumerable<Keyword>> _selectKeywords;

        private List<Modifier> _parsedModifiers;
        private List<UntranslatedStat> _parsedStats;
        private IEnumerable<Keyword> _keywords;
        private SkillPreParseResult _preParseResult;

        private SkillKeywordParser(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders,
            Func<SkillDefinition, IEnumerable<Keyword>> selectKeywords)
            => (_builderFactories, _metaStatBuilders, _selectKeywords) =
                (builderFactories, metaStatBuilders, selectKeywords);

        public static IPartialSkillParser CreateActive(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => new SkillKeywordParser(builderFactories, metaStatBuilders, d => d.ActiveSkill.Keywords);

        public static IPartialSkillParser CreateSupport(
            IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => new SkillKeywordParser(builderFactories, metaStatBuilders, d => d.SupportSkill.AddedKeywords);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new List<Modifier>();
            _parsedStats = new List<UntranslatedStat>();
            _preParseResult = preParseResult;
            _keywords = _selectKeywords(preParseResult.SkillDefinition);

            var isMainSkill = preParseResult.IsMainSkill.IsSet;
            var isAlwaysProjectile = HitIsAlwaysProjectile(preParseResult.LevelDefinition);
            var hitIsAreaDamage = HitDamageIsArea(preParseResult.LevelDefinition);
            var dotIsAreaDamage = AreaDamageOverTimeSkills.Contains(preParseResult.SkillDefinition.Id);

            IConditionBuilder CreatePartHasKeywordCondition(Keyword k)
                => PartHasKeywordCondition(isAlwaysProjectile, isMainSkill, k);

            AddKeywordModifiers(_metaStatBuilders.MainSkillHasKeyword, _ => isMainSkill);
            AddKeywordModifiers(_metaStatBuilders.MainSkillPartHasKeyword, CreatePartHasKeywordCondition);
            AddKeywordModifiers(_metaStatBuilders.MainSkillPartCastRateHasKeyword, CreatePartHasKeywordCondition);
            foreach (var damageSource in Enums.GetValues<DamageSource>())
            {
                var preCondition = damageSource != DamageSource.OverTime
                    ? KeywordPreCondition(hitIsAreaDamage, ExcludedKeywords[damageSource])
                    : KeywordPreCondition(dotIsAreaDamage, ExcludedKeywords[damageSource]);
                AddKeywordModifiers(k => _metaStatBuilders.MainSkillPartDamageHasKeyword(k, damageSource),
                    CreatePartHasKeywordCondition, preCondition);
            }
            AddKeywordModifiers(k => _metaStatBuilders.MainSkillPartAilmentDamageHasKeyword(k),
                CreatePartHasKeywordCondition, KeywordPreCondition(false, ExcludedKeywords[DamageSource.OverTime]));

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
                KeywordModifiers(_keywords, statFactory, conditionFactory, preCondition)
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

        private bool HitIsAlwaysProjectile(SkillLevelDefinition level)
            => MatchFlagStat(level, SkillStatIds.IsAlwaysProjectile);

        private bool HitDamageIsArea(SkillLevelDefinition level)
            => MatchFlagStat(level, SkillStatIds.IsAreaDamage);

        private bool MatchFlagStat(SkillLevelDefinition level, string statId)
        {
            var firstMatch = level.Stats.FirstOrDefault(s => s.StatId == statId);
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

        private Func<Keyword, bool> KeywordPreCondition(bool canBeArea, IEnumerable<Keyword> excludedKeywords)
        {
            return k => k == Keyword.AreaOfEffect ? canBeArea : !excludedKeywords.Contains(k);
        }
    }
}