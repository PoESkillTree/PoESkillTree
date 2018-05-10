using System.Collections.Generic;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class CoreStatGraph : IStatGraph
    {
        private readonly IStatNodeFactory _nodeFactory;
        private readonly PathDefinitionCollection _paths;

        private readonly Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>> _nodes =
            new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>();

        private readonly Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            _formNodeCollections =
                new Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>();

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

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeSelector selector) =>
            GetDisposableNode(selector);

        public IReadOnlyDictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>> Nodes => _nodes;

        public ISuspendableEventViewProvider<IObservableCollection<PathDefinition>> Paths => _paths;

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

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> 
            GetFormNodeCollection(FormNodeSelector selector) => 
            GetModifierNodeCollection(selector);

        public IReadOnlyDictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _formNodeCollections;

        public void RemoveFormNodeCollection(FormNodeSelector selector)
        {
            if (_formNodeCollections.Remove(selector))
            {
                _paths.Remove(selector.Path);
            }
        }

        public void AddModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            var selector = new FormNodeSelector(modifier.Form, PathDefinition.MainPath);
            var collection = GetModifierNodeCollection(selector);
            collection.Add(node, modifier);
            ModifierCount++;
        }

        public void RemoveModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            var selector = new FormNodeSelector(modifier.Form, PathDefinition.MainPath);
            var collection = GetModifierNodeCollection(selector);
            collection.Remove(node, modifier);
            ModifierCount--;
        }

        public int ModifierCount { get; private set; }
    }
}