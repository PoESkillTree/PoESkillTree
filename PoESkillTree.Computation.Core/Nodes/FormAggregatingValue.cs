using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class FormAggregatingValue : IValue
    {
        private readonly IStat _stat;
        private readonly Form _form;
        private readonly NodeValueAggregator _aggregator;

        public FormAggregatingValue(IStat stat, Form form, NodeValueAggregator aggregator)
        {
            _stat = stat;
            _form = form;
            _aggregator = aggregator;
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext)
        {
            return _aggregator(valueCalculationContext.GetValues(_form, _stat));
        }
    }
}