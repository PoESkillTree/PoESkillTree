using System.Collections.Generic;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StatBuilderStub : BuilderStub, IStatBuilder
    {
        private readonly Resolver<IStatBuilder> _resolver;

        public StatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        protected IStatBuilder This => this;

        public IStatBuilder Minimum => CreateStat(This, o => $"Minimum {o}");
        public IStatBuilder Maximum => CreateStat(This, o => $"Maximum {o}");

        public ValueBuilder Value => new ValueBuilder(CreateValue(This, o => $"Value of {o}"));

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"% of {o1} converted to {o2}");

        public IStatBuilder GainAs(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"% of {o1} added as {o2}");

        public IStatBuilder For(IEntityBuilder entity) =>
            CreateStat(This, entity, (o1, o2) => $"{o1} for {o2}");

        public IStatBuilder With(IKeywordBuilder keyword) =>
            CreateStat(This, keyword, (o1, o2) => $"{o1} (with {o2} skills)");

        public IStatBuilder NotWith(IKeywordBuilder keyword) =>
            CreateStat(This, keyword, (o1, o2) => $"{o1} (not with {o2} skills)");

        public IStatBuilder ChanceToDouble =>
            CreateStat(This, o => $"Chance to double {o}");

        public virtual IStatBuilder WithCondition(IConditionBuilder condition) =>
            CreateStat(This, condition, (s, c) => $"{s}\n  Condition: {c}");

        public IStatBuilder CombineWith(IStatBuilder other) =>
            CreateStat(This, other, (o1, o2) => $"ApplyOnce({o1}, {o2})");

        public IStatBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            new[] { new StatBuilderResult(new[] { new Stat(ToString()) }, parameters.ModifierSource, Funcs.Identity), };
    }
}