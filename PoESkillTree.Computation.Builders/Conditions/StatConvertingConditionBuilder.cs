using System;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class StatConvertingConditionBuilder : ConditionBuilderBase
    {
        private readonly StatConverter _statConverter;
        private readonly StatConverter _negatedStatConverter;
        private readonly Func<ResolveContext, IConditionBuilder> _resolver;

        public StatConvertingConditionBuilder(
            StatConverter statConverter,
            Func<ResolveContext, IConditionBuilder> resolver = null)
            : this(statConverter, statConverter, resolver)
        {
        }

        public StatConvertingConditionBuilder(
            StatConverter statConverter, StatConverter negatedStatConverter,
            Func<ResolveContext, IConditionBuilder> resolver = null)
        {
            _statConverter = statConverter;
            _negatedStatConverter = negatedStatConverter;
            _resolver = resolver;
        }

        public override IConditionBuilder Resolve(ResolveContext context) => _resolver?.Invoke(context) ?? this;

        public override IConditionBuilder Not =>
            new StatConvertingConditionBuilder(_negatedStatConverter, _statConverter, _resolver.AndThen(b => b.Not));

        public override ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(_statConverter);
    }
}