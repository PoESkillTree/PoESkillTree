using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
                skills.ToDictionary(d => d.Identifier));
        }

        public ISkillBuilderCollection this[params IKeywordBuilder[] keywords] =>
            new SkillBuilderCollection(_statFactory, keywords);

        public ISkillBuilder SummonSkeleton => GetSkill();
        public ISkillBuilder VaalSummonSkeletons => GetSkill();
        public ISkillBuilder RaiseSpectre => GetSkill();
        public ISkillBuilder RaiseZombie => GetSkill();
        public ISkillBuilder DetonateMines => GetSkill();

        private ISkillBuilder GetSkill([CallerMemberName] string identity = null) =>
            new SkillBuilder(_statFactory, CoreBuilder.Create(_skills.Value[identity]));
    }
}