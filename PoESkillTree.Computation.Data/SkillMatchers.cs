using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation containing <see cref="ISkillBuilder"/>. The contained skills
    /// are created from passed skill names.
    /// </summary>
    public class SkillMatchers : ReferencedMatchersBase<ISkillBuilder>
    {
        private readonly IReadOnlyList<string> _skillNames;
        private readonly Func<string, ISkillBuilder> _builderFactory;

        public SkillMatchers(IReadOnlyList<string> skillNames, Func<string, ISkillBuilder> builderFactory)
        {
            _skillNames = skillNames;
            _builderFactory = builderFactory;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection()
        {
            var coll = new ReferencedMatcherCollection<ISkillBuilder>();
            foreach (var skill in _skillNames)
            {
                coll.Add(skill.ToLowerInvariant(), _builderFactory(skill));
            }
            return coll;
        }
    }
}