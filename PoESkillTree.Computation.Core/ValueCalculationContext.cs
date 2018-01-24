using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class ValueCalculationContext : IValueCalculationContext
    {
        private readonly INodeRepository _nodeRepository;
        private readonly ICollection<(IStat stat, NodeType nodeType)> _calls =
            new HashSet<(IStat stat, NodeType nodeType)>();

        public ValueCalculationContext(INodeRepository nodeRepository)
        {
            _nodeRepository = nodeRepository;
        }

        public NodeValue? GetValue(IStat stat, NodeType nodeType = NodeType.Total)
        {
            _calls.Add((stat, nodeType));
            return _nodeRepository.GetNode(stat, nodeType)?.Value;
        }

        public IEnumerable<(IStat stat, NodeType nodeType)> Calls => _calls;

        public void Clear() => _calls.Clear();
    }
}