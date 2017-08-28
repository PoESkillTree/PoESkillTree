using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IAilmentBuilderCollection 
        : IBuilderCollection<IAilmentBuilder>, IEnumerable<IAilmentBuilder>
    {
    }
}