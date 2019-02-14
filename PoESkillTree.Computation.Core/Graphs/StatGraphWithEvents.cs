using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Decorating implementation of <see cref="IStatGraph"/> that triggers actions when nodes are added or removed.
    /// </summary>
    public class StatGraphWithEvents : IStatGraph
    {
        private readonly IStatGraph _decoratedGraph;
        private readonly Action<NodeSelector> _nodeAddedAction;
        private readonly Action<NodeSelector> _nodeRemovedAction;

        public StatGraphWithEvents(
            IStatGraph decoratedGraph, Action<NodeSelector> nodeAddedAction, Action<NodeSelector> nodeRemovedAction)
        {
            _decoratedGraph = decoratedGraph;
            _nodeAddedAction = nodeAddedAction;
            _nodeRemovedAction = nodeRemovedAction;
        }

        public IBufferingEventViewProvider<IObservableCollection<PathDefinition>> Paths => _decoratedGraph.Paths;

        public IBufferingEventViewProvider<ICalculationNode> GetNode(NodeSelector selector)
        {
            var nodeIsNew = !Nodes.ContainsKey(selector);
            var node = _decoratedGraph.GetNode(selector);
            if (nodeIsNew)
            {
                _nodeAddedAction(selector);
            }
            return node;
        }

        public IReadOnlyDictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>> Nodes =>
            _decoratedGraph.Nodes;

        public IBufferingEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(FormNodeSelector selector) =>
            _decoratedGraph.GetFormNodeCollection(selector);

        public IReadOnlyDictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _decoratedGraph.FormNodeCollections;

        public void RemoveNode(NodeSelector selector)
        {
            _decoratedGraph.RemoveNode(selector);
            _nodeRemovedAction(selector);
        }

        public void RemoveFormNodeCollection(FormNodeSelector selector) => 
            _decoratedGraph.RemoveFormNodeCollection(selector);

        public void AddModifier(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier) =>
            _decoratedGraph.AddModifier(node, modifier);

        public void RemoveModifier(IBufferingEventViewProvider<ICalculationNode> node, Modifier modifier) =>
            _decoratedGraph.RemoveModifier(node, modifier);

        public int ModifierCount => _decoratedGraph.ModifierCount;
    }
}