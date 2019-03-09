using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation for <see cref="IBuffBuilder"/>s.
    /// </summary>
    public class BuffMatchers : ReferencedMatchersBase<IBuffBuilder>
    {
        private IBuffBuilders Buff { get; }

        public BuffMatchers(IBuffBuilders buffBuilders)
        {
            Buff = buffBuilders;
        }

        protected override IReadOnlyList<ReferencedMatcherData> CreateCollection() =>
            new ReferencedMatcherCollection<IBuffBuilder>
            {
                { "fortify", Buff.Fortify },
                { "maim(ed)?", Buff.Maim },
                { "intimidate", Buff.Intimidate },
                { "taunt(ed)?", Buff.Taunt },
                { "blind", Buff.Blind },
                { "onslaught", Buff.Onslaught },
                { "unholy might", Buff.UnholyMight },
                { "phasing", Buff.Phasing },
                { "arcane surge", Buff.ArcaneSurge },
                { "tailwind", Buff.Tailwind },
                { "innervation", Buff.Innervation },
                { "impaled?", Buff.Impale },
                { "infusion", Buff.Infusion },
            }; // Add
    }
}