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
        private readonly ParametrisedStatConverter _negatedStatConverter;
        private readonly TParameter _parameter;

        public StatConvertingConditionBuilder(ParametrisedStatConverter statConverter, TParameter parameter)
            : this(statConverter, statConverter, parameter)
        {
        }

        public StatConvertingConditionBuilder(
            ParametrisedStatConverter statConverter, ParametrisedStatConverter negatedStatConverter,
            TParameter parameter)
        {
            _statConverter = statConverter;
            _negatedStatConverter = negatedStatConverter;
            _parameter = parameter;
        }

        public override IConditionBuilder Resolve(ResolveContext context) =>
            new StatConvertingConditionBuilder<TParameter>(_statConverter, _negatedStatConverter,
                _parameter.Resolve(context));

        public override IConditionBuilder Not =>
            new StatConvertingConditionBuilder<TParameter>(_negatedStatConverter, _statConverter, _parameter);

        public override ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(s => _statConverter(s, _parameter));
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

        public override ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(_statConverter);
    }
}