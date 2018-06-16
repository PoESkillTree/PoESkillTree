using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    internal static class StatBuilderHelper
    {
        public static ICoreStatBuilder CreateStatBuilder(string identity, params Entity[] entities) =>
            CreateStatBuilder(identity, new EntityBuilder(entities));

        public static ICoreStatBuilder CreateStatBuilder(string identity, IEntityBuilder entityBuilder)
        {
            IStat CreateStat(Entity entity) => new Stat(identity, entity);
            return new LeafCoreStatBuilder(CreateStat, entityBuilder);
        }

        public static ICoreStatBuilder CreateStatBuilder(IStat stat, IEntityBuilder entityBuilder) =>
            new LeafCoreStatBuilder(_ => stat, entityBuilder);
    }
}