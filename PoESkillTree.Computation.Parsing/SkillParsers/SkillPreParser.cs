using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SkillPreParser
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IMetaStatBuilders _metaStatBuilders;

        public SkillPreParser(SkillDefinitions skillDefinitions, IMetaStatBuilders metaStatBuilders)
            => (_skillDefinitions, _metaStatBuilders) = (skillDefinitions, metaStatBuilders);

        public SkillPreParseResult ParseActive(Skill activeSkill)
        {
            var definition = _skillDefinitions.GetSkillById(activeSkill.Id);
            var displayName = definition.BaseItem?.DisplayName ?? definition.ActiveSkill.DisplayName;
            return Parse(activeSkill, activeSkill, displayName);
        }

        public SkillPreParseResult ParseSupport(Skill activeSkill, Skill supportSkill)
        {
            var definition = _skillDefinitions.GetSkillById(supportSkill.Id);
            var displayName = definition.BaseItem?.DisplayName ?? supportSkill.Id;
            return Parse(activeSkill, supportSkill, displayName);
        }

        private SkillPreParseResult Parse(Skill mainSkill, Skill parsedSkill, string displayName)
        {
            var mainSkillDefinition = _skillDefinitions.GetSkillById(mainSkill.Id);
            var mainSkillLevel = mainSkillDefinition.Levels[mainSkill.Level];
            var parsedSkillDefinition = _skillDefinitions.GetSkillById(parsedSkill.Id);
            var parsedSkillLevel = parsedSkillDefinition.Levels[parsedSkill.Level];

            var localSource = new ModifierSource.Local.Skill(displayName);
            var globalSource = new ModifierSource.Global(localSource);
            var gemSource = new ModifierSource.Local.Gem(parsedSkill.ItemSlot, parsedSkill.SocketIndex, displayName);

            var hitDamageSource = DetermineHitDamageSource(mainSkillDefinition.ActiveSkill, mainSkillLevel);
            var hasSkillDamageOverTime = HasSkillDamageOverTime(mainSkillLevel);
            var isMainSkillStat = _metaStatBuilders.MainSkillSocket(mainSkill.ItemSlot, mainSkill.SocketIndex);

            return new SkillPreParseResult(parsedSkillDefinition, parsedSkillLevel,
                localSource, globalSource, gemSource,
                hitDamageSource, hasSkillDamageOverTime, isMainSkillStat);
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

        private static bool HasSkillDamageOverTime(SkillLevelDefinition level)
            => level.Stats.Any(s => SkillStatIds.DamageOverTimeRegex.IsMatch(s.StatId));
    }
}