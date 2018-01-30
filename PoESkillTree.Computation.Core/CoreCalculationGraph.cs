using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class CoreCalculationGraph : ICalculationGraph
    {
        private readonly INodeFactory _nodeFactory;
        private readonly INodeCollectionFactory _nodeCollectionFactory;

        private readonly Dictionary<IStat, Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>>
            _subgraphNodes =
                new Dictionary<IStat, Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>>();

        // Values are of type ModifierNodeCollection. Using it as TValue would break GetFormNodeCollections().
        private readonly Dictionary<IStat, Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>>
            _formCollections =
                new Dictionary<IStat, Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>>();

        public CoreCalculationGraph(INodeFactory nodeFactory, INodeCollectionFactory nodeCollectionFactory)
        {
            _nodeFactory = nodeFactory;
            _nodeCollectionFactory = nodeCollectionFactory;
        }

        public ISuspendableEvents Suspender { get; } = new NullSuspendableEvents();

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(IStat stat, NodeType nodeType) =>
            _subgraphNodes
                .GetOrAdd(stat, _ => CreateNodeDictionary())
                .GetOrAdd(nodeType, _ => _nodeFactory.Create(stat, nodeType));

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> GetNodes(IStat stat) =>
            _subgraphNodes
                .GetOrAdd(stat, _ => CreateNodeDictionary());

        private Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> CreateNodeDictionary() =>
            new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>();

        private ModifierNodeCollection GetModifierNodeCollection(IStat stat, Form form) =>
            (ModifierNodeCollection) _formCollections
                .GetOrAdd(stat, _ => CreateFormNodeDictionary())
                .GetOrAdd(form, _ => _nodeCollectionFactory.Create());

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(IStat stat, Form form) =>
            GetModifierNodeCollection(stat, form);

        public IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            GetFormNodeCollections(IStat stat) => 
            _formCollections.GetOrAdd(stat, _ => CreateFormNodeDictionary());

        private static Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            CreateFormNodeDictionary() =>
            new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>();

        public void RemoveNode(IStat stat, NodeType nodeType)
        {
            if (!_subgraphNodes.TryGetValue(stat, out var statNodes))
                return;
            if (!statNodes.TryGetValue(nodeType, out var node))
                return;
            node.DefaultView.Dispose();
            node.SuspendableView.Dispose();
            statNodes.Remove(nodeType);
        }

        public void RemoveFormNodeCollection(IStat stat, Form form)
        {
            if (_formCollections.TryGetValue(stat, out var formCollection))
            {
                formCollection.Remove(form);
            }
        }

        public void RemoveStat(IStat stat)
        {
            if (_subgraphNodes.TryGetValue(stat, out var statNodes) && statNodes.Any())
                throw new ArgumentException("Stats can only be removed when they have no nodes", nameof(stat));
            if (_formCollections.TryGetValue(stat, out var formCollections) && formCollections.Any())
                throw new ArgumentException("Stats can only be removed when they have no nodes", nameof(stat));
            _subgraphNodes.Remove(stat);
            _formCollections.Remove(stat);
        }

        public void AddModifier(IStat stat, Modifier modifier)
        {
            var collection = GetModifierNodeCollection(stat, modifier.Form);
            var node = _nodeFactory.Create(modifier.Value);
            collection.Add(modifier, node);
        }

        public bool RemoveModifier(IStat stat, Modifier modifier)
        {
            var collection = GetModifierNodeCollection(stat, modifier.Form);
            var node = collection.Remove(modifier);
            node?.DefaultView.Dispose();
            node?.SuspendableView.Dispose();
            return node != null;
        }
    }
}