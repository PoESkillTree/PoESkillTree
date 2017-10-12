using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Console
{
    public class SkillMatchers : ReferencedMatchersBase<ISkillBuilder>
    {
        private readonly ISkillBuilders _skillBuilders;

        public SkillMatchers(ISkillBuilders skillBuilders)
        {
            _skillBuilders = skillBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection() =>
            new ReferencedMatcherCollection<ISkillBuilder>
            {
                { "Blood Rage", _skillBuilders.BloodRage },
                { "Bone Offering", _skillBuilders.BoneOffering },
                { "Detonate Mines", _skillBuilders.DetonateMines },
                { "Flesh Offering", _skillBuilders.FleshOffering },
                { "Molten Shell", _skillBuilders.MoltenShell },
                { "Raise Spectre", _skillBuilders.RaiseSpectre },
                { "Raise Zombie", _skillBuilders.RaiseZombie },
                { "Spirit Offering", _skillBuilders.SpiritOffering },
                { "Summon Skeleton", _skillBuilders.SummonSkeleton },
                { "Vaal Summon Skeleton", _skillBuilders.VaalSummonSkeletons }
            };
    }
}