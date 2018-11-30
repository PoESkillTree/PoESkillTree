using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Utils;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinitionExtension
    {
        public SkillDefinitionExtension(
            SkillPartDefinitionExtension commonExtension, IReadOnlyDictionary<string, IEnumerable<Entity>> buffStats,
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

        public SkillPartDefinitionExtension CommonExtension { get; }
        public IReadOnlyList<SkillPartDefinitionExtension> PartExtensions { get; }
        public IReadOnlyList<string> PartNames { get; }
        public IReadOnlyDictionary<string, IEnumerable<Entity>> BuffStats { get; }
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

        public IEnumerable<UntranslatedStat> ModifyStats(IEnumerable<UntranslatedStat> stats)
        {
            var removedIds = _removedStats.ToHashSet();
            return _statReplacer(stats
                .Where(s => !removedIds.Contains(s.StatId))
                .Concat(_addedStats));
        }

        public IEnumerable<Keyword> ModifyKeywords(IEnumerable<Keyword> keywords)
            => keywords.Except(_removedKeywords).Union(_addedKeywords);
    }
}