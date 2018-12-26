using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Utils;

namespace PoESkillTree.GameModel.Skills
{
    /// <summary>
    /// Additional skill data that is not accessible through the game data. These are created in
    /// <see cref="SkillDefinitionExtensions"/>. The extension is used by <see cref="SkillJsonDeserializer"/> to
    /// extend the deserialized <see cref="SkillDefinition"/>.
    /// </summary>
    public class SkillDefinitionExtension
    {
        public SkillDefinitionExtension(
            SkillPartDefinitionExtension commonExtension, IReadOnlyDictionary<string, IReadOnlyList<Entity>> buffStats,
            IEnumerable<string> passiveStats, params (string name, SkillPartDefinitionExtension extension)[] parts)
        {
            CommonExtension = commonExtension;
            BuffStats = buffStats;
            PassiveStats = passiveStats;
            if (parts.Any())
            {
                PartExtensions = parts.Select(t => t.extension).ToList();
                PartNames = parts.Select(t => t.name).ToList();
            }
            else
            {
                PartExtensions = new[] { new SkillPartDefinitionExtension(), };
                PartNames = new[] { "" };
            }
        }

        /// <summary>
        /// Extension for all skill parts
        /// </summary>
        public SkillPartDefinitionExtension CommonExtension { get; }

        /// <summary>
        /// Extensions per skill part
        /// </summary>
        public IReadOnlyList<SkillPartDefinitionExtension> PartExtensions { get; }

        /// <summary>
        /// Names of the skill parts
        /// </summary>
        public IReadOnlyList<string> PartNames { get; }

        /// <summary>
        /// Stat ids of the skill's modifiers that are provided as part of the skill's buff, together with the Entities
        /// affected by the modifier
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<Entity>> BuffStats { get; }

        /// <summary>
        /// Stat ids of the skill's modifiers that are enabled even when the skill is not the main skill
        /// </summary>
        public IEnumerable<string> PassiveStats { get; }
    }

    public class SkillPartDefinitionExtension
    {
        private readonly IEnumerable<string> _removedStats;
        private readonly IEnumerable<UntranslatedStat> _addedStats;
        private readonly Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> _statReplacer;

        private readonly IEnumerable<Keyword> _removedKeywords;
        private readonly IEnumerable<Keyword> _addedKeywords;

        public SkillPartDefinitionExtension()
            : this(null, null, null, null, null)
        {
        }

        public SkillPartDefinitionExtension(
            Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> statReplacer,
            IEnumerable<Keyword> removedKeywords = null, IEnumerable<Keyword> addedKeywords = null)
            : this(null, null, statReplacer, removedKeywords, addedKeywords)
        {
        }

        public SkillPartDefinitionExtension(
            IEnumerable<string> removedStats,
            Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> statReplacer = null,
            IEnumerable<Keyword> removedKeywords = null, IEnumerable<Keyword> addedKeywords = null)
            : this(removedStats, null, statReplacer, removedKeywords, addedKeywords)
        {
        }

        public SkillPartDefinitionExtension(
            IEnumerable<UntranslatedStat> addedStats,
            Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> statReplacer = null,
            IEnumerable<Keyword> removedKeywords = null, IEnumerable<Keyword> addedKeywords = null)
            : this(null, addedStats, statReplacer, removedKeywords, addedKeywords)
        {
        }

        public SkillPartDefinitionExtension(
            IEnumerable<string> removedStats, IEnumerable<UntranslatedStat> addedStats,
            Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> statReplacer = null,
            IEnumerable<Keyword> removedKeywords = null, IEnumerable<Keyword> addedKeywords = null)
        {
            _removedStats = removedStats ?? new string[0];
            _addedStats = addedStats ?? new UntranslatedStat[0];
            _statReplacer = statReplacer ?? Funcs.Identity;
            _removedKeywords = removedKeywords ?? new Keyword[0];
            _addedKeywords = addedKeywords ?? new Keyword[0];
        }

        /// <summary>
        /// Modifies the stats of this skill part as specified by this extension.
        /// </summary>
        public IEnumerable<UntranslatedStat> ModifyStats(IEnumerable<UntranslatedStat> stats)
        {
            var removedIds = _removedStats.ToHashSet();
            return _statReplacer(stats
                .Where(s => !removedIds.Contains(s.StatId))
                .Concat(_addedStats));
        }

        /// <summary>
        /// Modifies the keywords of this skill part as specified by this extension.
        /// </summary>
        public IEnumerable<Keyword> ModifyKeywords(IEnumerable<Keyword> keywords)
            => keywords.Except(_removedKeywords).Union(_addedKeywords);
    }
}