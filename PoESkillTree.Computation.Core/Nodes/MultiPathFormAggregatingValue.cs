using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class MultiPathFormAggregatingValue : IValue
    {
        private readonly IStat _stat;
        private readonly Form _form;
        private readonly PathDefinition _path;
        private readonly NodeValueAggregator _aggregator;

        public MultiPathFormAggregatingValue(IStat stat, Form form, PathDefinition path, NodeValueAggregator aggregator)
        {
            _stat = stat;
            _form = form;
            _path = path;
            _aggregator = aggregator;
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext)
        {
            var stats = _stat.Concat(_path.ConversionStats).ToList();
            var paths = _path.ModifierSource.InfluencingSources
                .Select(s => new PathDefinition(s))
                .SelectMany(p => stats.Select(s => (s, p)));
            return _aggregator(valueCalculationContext.GetValues(_form, paths));
        }
    }
}