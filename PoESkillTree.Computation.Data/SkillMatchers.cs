using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation containing <see cref="ISkillBuilder"/>. The contained skills
    /// are created from passed SkillDefinitions.
    /// </summary>
    public class SkillMatchers : ReferencedMatchersBase<ISkillBuilder>
    {
        private static readonly IEnumerable<(string regex, string skillId)> ManualMatchers = new[]
        {
            ("ice golem summoned", "SummonIceGolem"),
            ("flame golem summoned", "SummonFireGolem"),
            ("lightning golem summoned", "SummonLightningGolem")
        };

        private readonly IReadOnlyList<SkillDefinition> _skills;
        private readonly Func<string, ISkillBuilder> _builderFactory;

        public SkillMatchers(IReadOnlyList<SkillDefinition> skills, Func<string, ISkillBuilder> builderFactory)
        {
            _skills = skills;
            _builderFactory = builderFactory;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection()
        {
            var coll = new ReferencedMatcherCollection<ISkillBuilder>();
            foreach (var skill in _skills.Where(d => !d.IsSupport))
            {
                var regex = skill.ActiveSkill.DisplayName.ToLowerInvariant();
                coll.Add(regex, _builderFactory(skill.Id));
            }
            foreach (var (regex, id) in ManualMatchers)
            {
                coll.Add(regex, _builderFactory(id));
            }
            return coll;
        }
    }
}