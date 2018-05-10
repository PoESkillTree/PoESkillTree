using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Graphs;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    internal static class NodeSelectorHelper
    {
        public static NodeSelector Selector(NodeType nodeType) => new NodeSelector(nodeType, PathDefinition.MainPath);

        public static FormNodeSelector Selector(Form form) => new FormNodeSelector(form, PathDefinition.MainPath);
    }
}