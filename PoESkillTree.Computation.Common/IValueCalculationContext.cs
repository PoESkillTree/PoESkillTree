using System.Collections.Generic;

namespace PoESkillTree.Computation.Common
{
    public interface IValueCalculationContext
    {
        NodeValue? GetValue(IStat stat, NodeType nodeType = NodeType.Total);

        IEnumerable<NodeValue?> GetValues(Form form, params IStat[] stats);
    }
}