using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface ICoreStatBuilder : IResolvable<ICoreStatBuilder>
    {
        ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder);

        IEnumerable<StatBuilderResult> Build(BuildParameters parameters);
    }
}