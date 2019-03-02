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

        void AddModifier(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier);
        void RemoveModifier(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier);
    }


    /// <summary>
    /// Read-only representation of the calculation subgraph of a stat.
    /// </summary>
    public interface IReadOnlyStatGraph
    {
        IBufferingEventViewProvider<IObservableCollection<PathDefinition>> Paths { get; }

        IBufferingEventViewProvider<ICalculationNode> GetNode(NodeSelector selector);

        IReadOnlyDictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>> Nodes { get; }

        IBufferingEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(FormNodeSelector selector);

        IReadOnlyDictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections { get; }

        int ModifierCount { get; }
    }
}