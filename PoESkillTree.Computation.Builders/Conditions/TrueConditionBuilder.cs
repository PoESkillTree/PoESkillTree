using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class TrueConditionBuilder : IConditionBuilder
    {
        public IConditionBuilder Resolve(ResolveContext context) => this;

        public IConditionBuilder And(IConditionBuilder condition) => condition;

        public IConditionBuilder Or(IConditionBuilder condition) => condition;

        public IConditionBuilder Not { get; } = new ValueConditionBuilder(false);

        public (StatConverter statConverter, IValue value) Build(BuildParameters parameters) =>
            (Funcs.Identity, new Constant((NodeValue?) true));
    }
}