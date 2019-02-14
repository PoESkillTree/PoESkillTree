using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Core implementation of <see cref="IStatGraph"/>.
    /// </summary>
    public class CoreStatGraph : IStatGraph
    {
        private readonly IStatNodeFactory _nodeFactory;
        private readonly PathDefinitionCollection _paths;

        private readonly Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>> _nodes =
            new Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>();

        private readonly Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
            _formNodeCollections =
                new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>();

        public CoreStatGraph(IStatNodeFactory nodeFactory, PathDefinitionCollection paths)
        {
            _nodeFactory = nodeFactory;
            _paths = paths;
        }

        private IDisposableNodeViewProvider GetDisposableNode(NodeSelector selector) =>
            (IDisposableNodeViewProvider) _nodes.GetOrAdd(selector, CreateDisposableNode);

        private IDisposableNodeViewProvider CreateDisposableNode(NodeSelector selector)
        {
            _paths.Add(selector.Path);
            return _nodeFactory.Create(selector);
        }

        public IBufferingEventViewProvider<ICalculationNode> GetNode(NodeSelector selector) =>
            GetDisposableNode(selector);

        public IReadOnlyDictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>> Nodes => _nodes;

        public IBufferingEventViewProvider<IObservableCollection<PathDefinition>> Paths => _paths;

        public void RemoveNode(NodeSelector selector)
        {
            if (!_nodes.ContainsKey(selector))
                return;
            var node = GetDisposableNode(selector);
            node.Dispose();
            _nodes.Remove(selector);
            _paths.Remove(selector.Path);
        }

        private ModifierNodeCollection GetModifierNodeCollection(FormNodeSelector selector) =>
            (ModifierNodeCollection) _formNodeCollections.GetOrAdd(selector, CreateModifierNodeCollection);

        private ModifierNodeCollection CreateModifierNodeCollection(FormNodeSelector selector)
        {
            _paths.Add(selector.Path);
            return _nodeFactory.Create(selector);
        }

        public IBufferingEventViewProvider<INodeCollection<Modifier>> 
            GetFormNodeCollection(FormNodeSelector selector) => 
            GetModifierNodeCollection(selector);

        public IReadOnlyDictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _formNodeCollections;

        public void RemoveFormNodeCollection(FormNodeSelector selector)
        {
            if (_formNodeCollections.Remove(selector))
            {
                _paths.Remove(selector.Path);
            }
        }

        public void AddModifier(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            var collection = GetModifierNodeCollection(modifier);
            collection.Add(node, modifier);
            ModifierCount++;
        }

        public void RemoveModifier(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            var collection = GetModifierNodeCollection(modifier);
            collection.Remove(node, modifier);
            ModifierCount--;
        }

        private ModifierNodeCollection GetModifierNodeCollection(Modifier modifier)
        {
            var path = new PathDefinition(modifier.Source.CanonicalSource);
            return GetModifierNodeCollection(new FormNodeSelector(modifier.Form, path));
        }

        public int ModifierCount { get; private set; }
    }
}