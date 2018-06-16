using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StatBuildersStub : StatBuilders
    {
        public override IDamageRelatedStatBuilder Accuracy => CreateDamageStat("Accuracy");

        public override IDamageRelatedStatBuilder CastSpeed => CreateDamageStat("Attack/Cast Speed");

        public override IPoolStatBuilders Pool => new PoolStatBuildersStub();
    }
}