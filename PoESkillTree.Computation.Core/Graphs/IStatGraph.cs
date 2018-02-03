using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IStatGraph : IReadOnlyStatGraph
    {
        void RemoveNode(NodeType nodeType);
        void RemoveFormNodeCollection(Form form);

        void AddModifier(ISuspendableEventViewProvider<IDisposableNode> node, Modifier modifier);
        void RemoveModifier(ISuspendableEventViewProvider<IDisposableNode> node, Modifier modifier);
        int ModifierCount { get; }
    }

    public interface IReadOnlyStatGraph
    {
        ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeType nodeType);
        IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> Nodes { get; }

        ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(Form form);
        IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>> FormNodeCollections { get; }
    }

}