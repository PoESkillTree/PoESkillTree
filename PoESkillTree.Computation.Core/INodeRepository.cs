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


    public class CleanableCalculationGraph
        : ICalculationGraph, ICalculationGraphCleaner
    {
        private readonly Dictionary<IStat, int> _modifierCounts;
        private readonly HashSet<IStat> _statsWithoutModifiers;
        private readonly HashSet<IStat> _knownStats;

        // decoratedGraph will be a SuspendableCalculationGraph
        public CleanableCalculationGraph(ICalculationGraph decoratedGraph)
        {
        }

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

        public void RemoveNode(IStat node, NodeType nodeType)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<Form, ModifierNodeCollection> GetFormNodeCollections(IStat stat)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void RemoveFormNodeCollection(IStat stat, Form form)
        {
            // Call decoratedGraph
            throw new System.NotImplementedException();
        }

        public void AddModifier(Modifier modifier)
        {
            /* _decoratedGraph.AddModifier(modifier)
             * - For each stat in Modifier.Stats
             *   - Increment _modifierCounts
             *   - Remove stat from _statsWithoutModifiers
             *   - Add stat to _knownStats
             */
        }

        public void RemoveModifier(Modifier modifier)
        {
            /* _decoratedGraph.RemoveModifier(modifier)
             * - For each stat in Modifier.Stats
             *   - Decrement _modifierCounts
             *   - Add stat to _statsWithoutModifiers if _modifierCounts[stat] == 0
             */
        }

        public void RemoveUnusedNodes()
        {
            /* - For each stat in _statsWithoutModifiers
             *   - subgraphNodes = _decoratedGraph.GetNodes(stat)
             *   - For each NodeType (top-down):
             *     - If subgraphNodes.TryGetNode(stat, nodeType, out var node)
             *       - If SubscriberCount == 0: _decoratedGraph.RemoveNode(stat, nodeType)
             *   - formNodeCollections = _decoratedGraphy.GetFormNodeCollections(stat)
             *   - For each (form, nodeCollection) in formCollections:
             *     - If SubscriberCount == 0: _decoratedGraph.Remove(form, nodeCollection)
             *   - If subgraphNodes.IsEmpty() && formNodeCollections.IsEmpty():
             *     - _statsWithoutModifiers.Remove(stat), _knownStats.Remove(stat)
             * (remove calls need to be done after iterating)
             * (ISuspendableEventViewProvider, ICalculationNode and INodeCollection need to implement "ICountsSubscribers"
             *  ISuspendableEventViewProvider sums those of both views, the others return
             *  ValueChanged/CollectionChanged.GetInvocationList().Length
             *  (or just "HasSubscribers"))
             */
        }
    }

    public class SuspendableCalculationGraph : ICalculationGraph
    {
        private readonly SuspendableEventsComposite _suspendable;

        // decoratedGraph will be a CoreCalculationGraph
        public SuspendableCalculationGraph(ICalculationGraph decoratedGraph)
        {
        }

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

        public void RemoveNode(IStat node, NodeType nodeType)
        {
            // r = _decoratedGraph.GetNode(stat, noeType)
            // _suspendable.Remove(r.Suspender)
            // _decoratedProviderRepository.RemoveNode(stat, nodeType)
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<Form, ModifierNodeCollection> GetFormNodeCollections(IStat stat)
        {
            // return _decoratedGraph.GetFormNodeCollections(stat)
            throw new System.NotImplementedException();
        }

        public void RemoveFormNodeCollection(IStat stat, Form form)
        {
            // r = _decoratedGraph.GetFormNodeCollection(stat, form)
            // _suspendable.Remove(r.Suspender)
            // _decoratedGraph.RemoveFormNodeCollection(stat, form)
        }

        public void AddModifier(Modifier modifier)
        {
            // _decoratedGraph.AddModifier(modifier)
        }

        public void RemoveModifier(Modifier modifier)
        {
            // _decoratedGraph.RemoveModifier(modifier)
        }
    }

    public class CoreCalculationGraph : ICalculationGraph
    {
        private readonly Dictionary<IStat, Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>>
            _subgraphNodes;

        private readonly Dictionary<IStat, Dictionary<Form, ModifierNodeCollection>> _formCollections;

        public CoreCalculationGraph(INodeFactory nodeFactory, INodeCollectionFactory nodeCollectionFactory)
        {
        }

        public ISuspendableEvents Suspender { get; } // NullSuspendableEvents

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(IStat stat, NodeType nodeType)
        {
            //- If entry does not exist in _subgraphNodes:
            //  - Create nodes with _nodeFactory.Create(IStat, NodeType)
            //  - Add dictionary entry
            //- Return entry
            throw new System.NotImplementedException();
        }

        private ModifierNodeCollection GetModifierNodeCollection(IStat stat, Form form)
        {
            //- If entry does not exist in _formCollections:
            //  - Create nodes with _nodeCollectionFactory.Create(IStat, Form)
            //  - Add dictionary entry
            //- Return entry
            throw new System.NotImplementedException();
        }

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(IStat stat, Form form)
        {
            // return GetModifierNodeCollection(stat, form);
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> GetNodes(IStat stat)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveNode(IStat node, NodeType nodeType)
        {
            // - node = _subgraphNodes[stat][nodeType]
            // - node.DefaultView.Dispose(), node.SuspendableView.Dispose()
            // - _subgraphNodes[stat].Remove(nodeType)
            // - If _subgraphNodes[stat].IsEmpty(): _subgraphNodes.Remove(stat)
            throw new System.NotImplementedException();
        }

        public IReadOnlyDictionary<Form, ModifierNodeCollection> GetFormNodeCollections(IStat stat)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveFormNodeCollection(IStat stat, Form form)
        {
            // - _formCollections[stat].Remove(form)
            // - If _formCollections[stat].IsEmpty(): _formCollections.Remove(stat)
        }

        public void AddModifier(Modifier modifier)
        {
            /* - For each stat in modifier.Stats
             *   - collection = GetModifierNodeCollection(stat, modifier.Form)
             *   - node = _nodeFactory.Create(modifier.Value)
             *   - ModifierNodeCollection.AddModifier(modifier, node)
             */
        }

        public void RemoveModifier(Modifier modifier)
        {
            /* - For each stat in modifier.Stats
             *   - collection = GetModifierNodeCollection(stat, modifier.Form)
             *   - node = ModifierNodeCollection.RemoveModifier(modifier)
             *   - node.DefaultView.Dispose(), node.SuspendableView.Dispose()
             */
        }
    }
}