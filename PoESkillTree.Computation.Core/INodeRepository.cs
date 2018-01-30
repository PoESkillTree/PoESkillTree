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
        private readonly Dictionary<IStat, int> _modifierCounts;
        private readonly HashSet<IStat> _statsWithoutModifiers;
        private readonly HashSet<IStat> _knownStats;

        // decoratedGraph will be a SuspendableCalculationGraph
        public PrunableCalculationGraph(ICalculationGraph decoratedGraph)
        {
        }

        // For multiple ICalculationGraphs in a decoration chain, lower levels need to call methods on the top level
        // when calling other methods than the currently decorated one.
        // E.g. if an instance decorating a PrunableCalculationGraph does something when RemoveNode() is called, that
        // also needs to be done when RemoveNode() is called from PrunableCalculationGraph.RemoveUnusedNodes()
        public ICalculationGraph TopGraph { get; set; }

        public ISuspendableEvents Suspender { get; } // => _decoratedGraphy.Suspender

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(IStat stat, NodeType nodeType)
        {
            /* - If _knownStats.Add(stat): _statsWithoutModifiers.Add(stat)
             * - Return _decoratedGraph.GetNode(IStat, NodeType)
             */
            throw new System.NotImplementedException();
        }

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(IStat stat, Form form)
        {
            /* - If _knownStats.Add(stat): _statsWithoutModifiers.Add(stat)
             * - Return _decoratedGraph.GetFormNodeCollection(IStat, Form)
             */
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> GetNodes(IStat stat)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void RemoveNode(IStat stat, NodeType nodeType)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            GetFormNodeCollections(IStat stat)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void RemoveFormNodeCollection(IStat stat, Form form)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void RemoveStat(IStat stat)
        {
            // _statsWithoutModifiers.Remove(stat), _knownStats.Remove(stat)
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void AddModifier(IStat stat, Modifier modifier)
        {
            /* _decoratedGraph.AddModifier(modifier)
             *   - Increment _modifierCounts
             *   - Remove stat from _statsWithoutModifiers
             *   - Add stat to _knownStats
             */
            throw new System.NotImplementedException();
        }

        public bool RemoveModifier(IStat stat, Modifier modifier)
        {
            /* if _decoratedGraph.RemoveModifier(modifier):
             *   - Decrement _modifierCounts
             *   - Add stat to _statsWithoutModifiers if _modifierCounts[stat] == 0
             */
            throw new System.NotImplementedException();
        }

        public void RemoveUnusedNodes()
        {
            /* - For each stat in _statsWithoutModifiers
             *   - subgraphNodes = TopGraph.GetNodes(stat)
             *   - For each NodeType (top-down):
             *     - If subgraphNodes.TryGetNode(stat, nodeType, out var node)
             *       - If node.SubscriberCount == 0: TopGraph.RemoveNode(stat, nodeType)
             *   - formNodeCollections = TopGraph.GetFormNodeCollections(stat)
             *   - For each (form, nodeCollection) in formCollections:
             *     - If nodeCollection.SubscriberCount == 0: TopGraph.Remove(stat, form)
             *   - If subgraphNodes.IsEmpty() && formNodeCollections.IsEmpty():
             *     - TopGraph.RemoveStat(stat)
             * (remove calls need to be done after iterating)
             * (ISuspendableEventViewProvider and/or ICalculationNode and INodeCollection need to implement
             *  "ICountsSubscribers". SubscriberCount returns ValueChanged/CollectionChanged.GetInvocationList().Length)
             */
            throw new System.NotImplementedException();
        }
    }

    public class SuspendableCalculationGraph : ICalculationGraph
    {
        private readonly SuspendableEventsComposite _suspendable;

        // decoratedGraph will be a CoreCalculationGraph
        public SuspendableCalculationGraph(ICalculationGraph decoratedGraph)
        {
        }

        public ICalculationGraph TopGraph { get; set; }

        public ISuspendableEvents Suspender { get; } // => _suspendable

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(IStat stat, NodeType nodeType)
        {
            // r = _decoratedGraph.GetNode(stat, nodeType)
            // _suspendable.Add(r.Suspender)
            // return r
            throw new System.NotImplementedException();
        }

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(IStat stat, Form form)
        {
            // r = _decoratedGraph.GetFormNodeCollection(stat, form)
            // _suspendable.Add(r.Suspender)
            // return r
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> GetNodes(IStat stat)
        {
            // return _decoratedGraph.GetNodes(stat)
            throw new System.NotImplementedException();
        }

        public void RemoveNode(IStat stat, NodeType nodeType)
        {
            // r = TopGraph.GetNode(stat, noeType)
            // _suspendable.Remove(r.Suspender)
            // _decoratedProviderRepository.RemoveNode(stat, nodeType)
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            GetFormNodeCollections(IStat stat)
        {
            // return _decoratedGraph.GetFormNodeCollections(stat)
            throw new System.NotImplementedException();
        }

        public void RemoveFormNodeCollection(IStat stat, Form form)
        {
            // r = TopGraph.GetFormNodeCollection(stat, form)
            // _suspendable.Remove(r.Suspender)
            // _decoratedGraph.RemoveFormNodeCollection(stat, form)
            throw new System.NotImplementedException();
        }

        public void RemoveStat(IStat stat)
        {
            // Call _decoratedGraph
            throw new System.NotImplementedException();
        }

        public void AddModifier(IStat stat, Modifier modifier)
        {
            // _decoratedGraph.AddModifier(modifier)
            // - r = TopGraph.GetFormNodeCollection(stat, modifier.Form)
            // - _suspendable.Add(r.Suspender)
            throw new System.NotImplementedException();
        }

        public bool RemoveModifier(IStat stat, Modifier modifier)
        {
            // _decoratedGraph.RemoveModifier(modifier)
            throw new System.NotImplementedException();
        }
    }
}