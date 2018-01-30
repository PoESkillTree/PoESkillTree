using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeRepository
    {
        // Asking for non-existing nodes leads to their creation

        // stat selects the stat subgraph, nodeType the node in it.
        // With conversions and/or sources:
        // - Increase, More: the node on the unconverted, Global path.
        // - Base, BaseOverride, BaseSet, Base Add: the unconverted base node.
        // - UncappedSubtotal: The node that sums all paths.
        // - Subtotal, TotalOverride, Total: There should only be one.
        // If stat is null, this returns a node that always has a value of null
        ICalculationNode GetNode([CanBeNull] IStat stat, NodeType nodeType = NodeType.Total);

        // stat selects the stat subgraph, nodeType the node in it.
        // Only one NodeType from Total, Subtotal and UncappedSubtotal make sense, probably Uncapped Subtotal as
        // that's where these path subgraphs end up. BaseOverride, BaseSet, BaseAdd and TotalOverride don't make sense.
        // Returns all nodes by conversion path and source.
        //INodeCollection<NodePathProperty> GetPathNodes(IStat stat, NodeType nodeType = NodeType.Total);
        // NodePathProperty: Contains the path's definition
        // - Its IModifierSource (only with the information that is the same for all modifiers of the path)
        // - The IStats on its conversion path (the node's IStat itself if unconverted)

        // Returns the form node collection of stat
        INodeCollection<Modifier> GetFormNodeCollection(IStat stat, Form form);
    }


    public class PrunableCalculationGraph
        : ICalculationGraph, ICalculationGraphPruner
    {
        private readonly HashSet<IStat> _statsWithoutModifiers;

        public PrunableCalculationGraph(ICalculationGraph decoratedGraph)
        {
        }

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator()
        {
            // Call _decoratedGraph
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Call _decoratedGraph
        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs { get; }

        public IReadOnlyStatGraph GetOrAdd(IStat stat)
        {
            /* - If !StatGraphs.Contains(stat): _statsWithoutModifiers.Add(stat)
             * - Return _decoratedGraph.GetOrAdd(NodeType)
             */
            throw new System.NotImplementedException();
        }

        public void Remove(IStat stat)
        {
            // _statsWithoutModifiers.Remove(stat)
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void AddModifier(Modifier modifier)
        {
            /* For each stat in modifier.Stats:
             *   _statsWithoutModifiers.Remove(stat)
             * _decoratedGraph.AddModifier(modifier)
             */
            throw new System.NotImplementedException();
        }

        public void RemoveModifier(Modifier modifier)
        {
            /* _decoratedGraph.RemoveModifier(modifier)
             * For each stat in modifier.Stats:
             *   If StatGraphs.Contains(stat) && StatGraphs[stat].ModifierCount == 0:
             *     _statsWithoutModifiers.Add(stat)
             */
            throw new System.NotImplementedException();
        }

        public void RemoveUnusedNodes()
        {
            /* - For each stat in _statsWithoutModifiers
             *   - statGraph = StatGraphs[stat]
             *   - For each NodeType (top-down):
             *     - If statGraph.Nodes.TryGetNode(stat, nodeType, out var node)
             *       - If node.SubscriberCount == 0: statGraph.RemoveNode(nodeType)
             *   - For each (form, nodeCollection) in statGraph.FormNodeCollections:
             *     - If nodeCollection.SubscriberCount == 0: statGraph.Remove(form)
             *   - If statGraph.Nodes.IsEmpty() && statGraph.FormNodeCollections.IsEmpty():
             *     - TopGraph.RemoveStat(stat)
             * (remove calls need to be done after iterating)
             * (ISuspendableEventViewProvider and/or ICalculationNode and INodeCollection need to implement
             *  "ICountsSubscribers". SubscriberCount returns ValueChanged/CollectionChanged.GetInvocationList().Length)
             */
            throw new System.NotImplementedException();
        }
    }
}