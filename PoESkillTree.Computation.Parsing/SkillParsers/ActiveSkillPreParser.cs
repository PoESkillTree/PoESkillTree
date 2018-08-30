using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillPreParser
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IMetaStatBuilders _metaStatBuilders;

        private List<UntranslatedStat> _parsedStats;

        public ActiveSkillPreParser(SkillDefinitions skillDefinitions, IMetaStatBuilders metaStatBuilders)
            => (_skillDefinitions, _metaStatBuilders) = (skillDefinitions, metaStatBuilders);

        public (ActiveSkillPreParseResult preParseResult, IEnumerable<UntranslatedStat> parsedStats) Parse(Skill skill)
        {
            _parsedStats = new List<UntranslatedStat>();

            var definition = _skillDefinitions.GetSkillById(skill.Id);
            var activeSkill = definition.ActiveSkill;
            var level = definition.Levels[skill.Level];

            var displayName = definition.BaseItem?.DisplayName ??
                              (definition.IsSupport ? skill.Id : definition.ActiveSkill.DisplayName);
            var localSource = new ModifierSource.Local.Skill(displayName);
            var globalSource = new ModifierSource.Global(localSource);
            var gemSource = new ModifierSource.Local.Gem(skill.ItemSlot, skill.SocketIndex, displayName);

            var hitDamageSource = DetermineHitDamageSource(activeSkill, level);
            var hasSkillDamageOverTime = HasSkillDamageOverTime(level);

            var isMainSkillStat = _metaStatBuilders.MainSkillSocket(skill.ItemSlot, skill.SocketIndex);

            var preParseResult = new ActiveSkillPreParseResult(definition, localSource, globalSource, gemSource,
                hitDamageSource, hasSkillDamageOverTime, isMainSkillStat);
            var result = (preParseResult, _parsedStats);
            _parsedStats = null;
            return result;
        }

        private static DamageSource? DetermineHitDamageSource(
            ActiveSkillDefinition activeSkill, SkillLevelDefinition level)
        {
            if (activeSkill.ActiveSkillTypes.Contains(ActiveSkillType.Attack))
                return DamageSource.Attack;
            var statIds = level.Stats.Select(s => s.StatId);
            foreach (var statId in statIds)
            {
                var match = SkillStatIds.HitDamageRegex.Match(statId);
                if (match.Success)
                    return Enums.Parse<DamageSource>(match.Groups[1].Value, true);
                if (statId == SkillStatIds.DealsSecondaryDamage)
                    return DamageSource.Secondary;
            }
            return null;
        }

        private bool HasSkillDamageOverTime(SkillLevelDefinition level)
        {            
            var firstMatch = level.Stats.Where(s => SkillStatIds.DamageOverTimeRegex.IsMatch(s.StatId))
                .Cast<UntranslatedStat?>()
                .FirstOrDefault();
            if (firstMatch is UntranslatedStat stat)
            {
                _parsedStats.Add(stat);
                return true;
            }
            return false;
        }
    }
}