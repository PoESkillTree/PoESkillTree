using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.PathTotal"/>.
    /// </summary>
    public class PathTotalValue : IValue
    {
        private readonly IStat _stat;
        private readonly PathDefinition _path;

        public PathTotalValue(IStat stat, PathDefinition path)
        {
            _stat = stat;
            _path = path;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var @base = context.GetValue(_stat, NodeType.Base, _path);
            if (@base is null)
            {
                return null;
            }

            var increase = context.GetValue(_stat, NodeType.Increase, _path) ?? new NodeValue(0);
            var more = context.GetValue(_stat, NodeType.More, _path) ?? new NodeValue(1);
            return @base * (1 + increase) * more;
        }
    }
}