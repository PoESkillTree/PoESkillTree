using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    public interface IStatGraph : IReadOnlyStatGraph
    {
        // TODO Add path parameter
        void RemoveNode(NodeType nodeType);
        void RemoveFormNodeCollection(Form form);

        void AddModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier);
        void RemoveModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier);
        int ModifierCount { get; }
    }

    public interface IReadOnlyStatGraph
    {
        ISuspendableEventViewProvider<IObservableCollection<PathDefinition>> Paths { get; }

        ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeType nodeType, PathDefinition path);
        // TODO Add path to dictionary
        IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> Nodes { get; }

        ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(Form form, PathDefinition path);
        // TODO Add path to dictionary
        IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>> FormNodeCollections { get; }
    }

}