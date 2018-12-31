using System;
using System.Diagnostics;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Selects a node in an <see cref="IReadOnlyStatGraph"/>/<see cref="IStatGraph"/> using
    /// a <see cref="NodeType"/> and <see cref="PathDefinition"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(NodeType) + "}, {" + nameof(Path) + "}")]
    public class NodeSelector : ValueObject
    {
        private static readonly NodeType[] MainPathOnlyNodeTypes =
            { NodeType.Total, NodeType.Subtotal, NodeType.UncappedSubtotal, NodeType.TotalOverride };

        public NodeSelector(NodeType nodeType, PathDefinition path)
        {
            if (!path.IsMainPath && MainPathOnlyNodeTypes.Contains(nodeType))
                throw new ArgumentException($"{nodeType} is only allowed with the main path");

            NodeType = nodeType;
            Path = path;
        }

        public NodeType NodeType { get; }
        public PathDefinition Path { get; }

        protected override object ToTuple() => (NodeType, Path);

        public void Deconstruct(out NodeType nodeType, out PathDefinition path) => 
            (nodeType, path) = (NodeType, Path);
    }
}