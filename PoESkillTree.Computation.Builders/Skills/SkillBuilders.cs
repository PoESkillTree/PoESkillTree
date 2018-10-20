using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.GameModel.Skills;

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
                skills.Where(d => !d.IsSupport).ToDictionary(d => d.Id));
        }

        public ISkillBuilderCollection this[params IKeywordBuilder[] keywords]
            => new SkillBuilderCollection(_statFactory, keywords, _skills.Value.Values);

        public ISkillBuilder SummonSkeleton => FromId("SummonSkeletons");
        public ISkillBuilder VaalSummonSkeletons => FromId("VaalSummonSkeletons");
        public ISkillBuilder RaiseSpectre => FromId("RaiseSpectre");
        public ISkillBuilder RaiseZombie => FromId("RaiseZombie");
        public ISkillBuilder DetonateMines => FromId("GemDetonateMines");

        public ISkillBuilder FromId(string skillId)
            => new SkillBuilder(_statFactory, CoreBuilder.Create(_skills.Value[skillId]));
    }
}