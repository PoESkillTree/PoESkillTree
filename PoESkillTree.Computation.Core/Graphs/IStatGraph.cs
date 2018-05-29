using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Representation of the calculation subgraph of a stat.
    /// </summary>
    public interface IStatGraph : IReadOnlyStatGraph
    {
        void RemoveNode(NodeSelector selector);
        void RemoveFormNodeCollection(FormNodeSelector selector);

        void AddModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier);
        void RemoveModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier);
        int ModifierCount { get; }
    }


    /// <summary>
    /// Read-only representation of the calculation subgraph of a stat.
    /// </summary>
    public interface IReadOnlyStatGraph
    {
        ISuspendableEventViewProvider<IObservableCollection<PathDefinition>> Paths { get; }

        ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeSelector selector);

        IReadOnlyDictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>> Nodes { get; }

        ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(FormNodeSelector selector);

        IReadOnlyDictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections { get; }
    }
}