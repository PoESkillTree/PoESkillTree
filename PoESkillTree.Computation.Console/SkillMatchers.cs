using System.Collections.Generic;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Console
{
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