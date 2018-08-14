using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Builders.Skills
{
    public class SkillBuilders : ISkillBuilders
    {
        private readonly IStatFactory _statFactory;
        private readonly Lazy<IReadOnlyDictionary<string, SkillDefinition>> _skills;

        public SkillBuilders(IStatFactory statFactory, IReadOnlyList<SkillDefinition> skills)
        {
            _statFactory = statFactory;
            _skills = new Lazy<IReadOnlyDictionary<string, SkillDefinition>>(() =>
                skills.ToDictionary(d => d.SkillName.ToLowerInvariant()));
        }

        public ISkillBuilderCollection this[params IKeywordBuilder[] keywords] =>
            new SkillBuilderCollection(_statFactory, keywords, SelectSkills);

        private IEnumerable<string> SelectSkills(IEnumerable<Keyword> keywords)
        {
            var keywordList = keywords.ToList();
            return _skills.Value
                .Where(p => p.Value.Keywords.ContainsAll(keywordList))
                .Select(p => p.Key);
        }

        public ISkillBuilder SummonSkeleton => FromName("summon skeleton");
        public ISkillBuilder VaalSummonSkeletons => FromName("vaal summon skeletons");
        public ISkillBuilder RaiseSpectre => FromName("raise spectre");
        public ISkillBuilder RaiseZombie => FromName("raise zombie");
        public ISkillBuilder DetonateMines => FromName("detonate mines");

        public ISkillBuilder FromName(string skillName) =>
            new SkillBuilder(_statFactory, CoreBuilder.Create(_skills.Value[skillName.ToLowerInvariant()]));
    }
}