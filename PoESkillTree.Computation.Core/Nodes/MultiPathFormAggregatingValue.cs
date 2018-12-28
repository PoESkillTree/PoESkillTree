using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType"/>s that aggregate <see cref="Form"/> nodes from multiple paths.
    /// (<see cref="NodeType.Increase"/> and <see cref="NodeType.More"/>)
    /// </summary>
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
            var paths =
                from source in _path.ModifierSource.InfluencingSources
                let path = new PathDefinition(source)
                from stat in _path.ConversionStats.Prepend(_stat)
                select (stat, path);
            return _aggregator(valueCalculationContext.GetValues(_form, paths));
        }
    }
}