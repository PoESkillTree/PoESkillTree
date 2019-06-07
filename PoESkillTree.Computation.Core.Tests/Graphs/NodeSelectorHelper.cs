using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    internal static class NodeSelectorHelper
    {
        public static NodeSelector Selector(NodeType nodeType) => new NodeSelector(nodeType, PathDefinition.MainPath);

        public static FormNodeSelector Selector(Form form) => new FormNodeSelector(form, PathDefinition.MainPath);
    }
}