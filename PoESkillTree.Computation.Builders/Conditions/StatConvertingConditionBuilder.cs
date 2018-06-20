using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class StatConvertingConditionBuilder<TParameter> : ConditionBuilderBase
        where TParameter: IResolvable<TParameter>
    {
        public delegate IStatBuilder ParametrisedStatConverter(IStatBuilder stat, TParameter parameter);

        private readonly ParametrisedStatConverter _statConverter;
        private readonly TParameter _parameter;

        public StatConvertingConditionBuilder(ParametrisedStatConverter statConverter, TParameter parameter)
        {
            _statConverter = statConverter;
            _parameter = parameter;
        }

        public override IConditionBuilder Resolve(ResolveContext context) =>
            new StatConvertingConditionBuilder<TParameter>(_statConverter, _parameter.Resolve(context));

        public override IConditionBuilder Not => this;

        public override (StatConverter statConverter, IValue value) Build(BuildParameters parameters) =>
            (s => _statConverter(s, _parameter), new Constant(true));
    }

    public class StatConvertingConditionBuilder : ConditionBuilderBase
    {
        private readonly StatConverter _statConverter;
        private readonly StatConverter _negatedStatConverter;

        public StatConvertingConditionBuilder(StatConverter statConverter) : this(statConverter, statConverter)
        {
        }

        public StatConvertingConditionBuilder(StatConverter statConverter, StatConverter negatedStatConverter)
        {
            _statConverter = statConverter;
            _negatedStatConverter = negatedStatConverter;
        }

        public override IConditionBuilder Resolve(ResolveContext context) => this;

        public override IConditionBuilder Not =>
            new StatConvertingConditionBuilder(_negatedStatConverter, _statConverter);

        public override (StatConverter statConverter, IValue value) Build(BuildParameters parameters) =>
            (_statConverter, new Constant(true));
    }
}