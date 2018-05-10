using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class NodeSelector
    {
        public NodeSelector(NodeType nodeType, PathDefinition path)
        {
            NodeType = nodeType;
            Path = path;
        }

        public NodeType NodeType { get; }
        public PathDefinition Path { get; }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is NodeSelector other && Equals(other));

        private bool Equals(NodeSelector other) =>
            NodeType.Equals(other.NodeType) && Path.Equals(other.Path);

        public override int GetHashCode() =>
            (NodeType, Path).GetHashCode();
    }
}