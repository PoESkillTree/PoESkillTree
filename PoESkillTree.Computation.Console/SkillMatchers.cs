using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Console
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation containing <see cref="ISkillBuilder"/>. The contained skills
    /// are simply created from a fixed array of a few skill names.
    /// </summary>
    public class SkillMatchers : ReferencedMatchersBase<ISkillBuilder>
    {
        protected override IEnumerable<ReferencedMatcherData> CreateCollection()
        {
            string[] skills =
            {
                "Blood Rage", "Bone Offering", "Detonate Mines", "Flesh Offering", "Molten Shell", "Raise Spectre",
                "Raise Zombie", "Spirit Offering", "Summon Skeleton", "Vaal Summon Skeleton", "Frost Blades"
            };
            var coll = new ReferencedMatcherCollection<ISkillBuilder>();
            foreach (var skill in skills)
            {
                coll.Add(skill, new SkillBuilderStub(skill, (c, _) => c));
            }
            return coll;
        }
    }
}