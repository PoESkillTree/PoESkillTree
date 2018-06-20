using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class ConstantConditionBuilder : IConditionBuilder
    {
        public static readonly IConditionBuilder True = new ConstantConditionBuilder(true);
        public static readonly IConditionBuilder False = new ConstantConditionBuilder(false);

        public static IConditionBuilder Create(bool value) => value ? True : False;

        private readonly bool _value;

        private ConstantConditionBuilder(bool value) => _value = value;

        public IConditionBuilder Resolve(ResolveContext context) => this;

        public IConditionBuilder And(IConditionBuilder condition) => _value ? condition : False;

        public IConditionBuilder Or(IConditionBuilder condition) => _value ? True : condition;

        public IConditionBuilder Not => Create(!_value);

        public (StatConverter statConverter, IValue value) Build(BuildParameters parameters) =>
            (Funcs.Identity, new Constant(_value));
    }
}