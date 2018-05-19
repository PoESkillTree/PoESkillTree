using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType"/>s that aggregate <see cref="Form"/> nodes from a single path.
    /// (<see cref="NodeType.BaseSet"/>, <see cref="NodeType.BaseAdd"/>, <see cref="NodeType.BaseOverride"/> and
    /// <see cref="NodeType.TotalOverride"/>)
    /// </summary>
    public class FormAggregatingValue : IValue
    {
        private readonly IStat _stat;
        private readonly Form _form;
        private readonly PathDefinition _path;
        private readonly NodeValueAggregator _aggregator;

        public FormAggregatingValue(IStat stat, Form form, PathDefinition path, NodeValueAggregator aggregator)
        {
            _stat = stat;
            _form = form;
            _path = path;
            _aggregator = aggregator;
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) => 
            _aggregator(valueCalculationContext.GetValues(_form, _stat, _path));
    }
}