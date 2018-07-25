using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Resolving
{
    public class MatchContexts : IMatchContexts
    {
        public MatchContexts(IStatFactory statFactory)
        {
            References = new ReferenceMatchContext(statFactory);
            Values = new ValueMatchContext();
        }

        public IMatchContext<IReferenceConverter> References { get; }

        public IMatchContext<ValueBuilder> Values { get; }

        private class ValueMatchContext : IMatchContext<ValueBuilder>
        {
            public ValueBuilder this[int index] =>
                new ValueBuilder(new UnresolvedValueBuilder("Values[{index}]", c => c.ValueContext[index]));

            public ValueBuilder First =>
                new ValueBuilder(new UnresolvedValueBuilder("Values.First", c => c.ValueContext.First));

            public ValueBuilder Last =>
                new ValueBuilder(new UnresolvedValueBuilder("Values.Last", c => c.ValueContext.Last));

            public ValueBuilder Single =>
                new ValueBuilder(new UnresolvedValueBuilder("Values.Single", c => c.ValueContext.Single));
        }

        private class ReferenceMatchContext : IMatchContext<IReferenceConverter>
        {
            private readonly IStatFactory _statFactory;

            public ReferenceMatchContext(IStatFactory statFactory)
            {
                _statFactory = statFactory;
            }

            public IReferenceConverter this[int index] =>
                new UnresolvedReferenceConverter(_statFactory, $"References[{index}]", c => c.ReferenceContext[index]);

            public IReferenceConverter First =>
                new UnresolvedReferenceConverter(_statFactory, $"References.First", c => c.ReferenceContext.First);

            public IReferenceConverter Last =>
                new UnresolvedReferenceConverter(_statFactory, $"References.Last", c => c.ReferenceContext.Last);

            public IReferenceConverter Single =>
                new UnresolvedReferenceConverter(_statFactory, $"References.Single", c => c.ReferenceContext.Single);
        }
    }
}