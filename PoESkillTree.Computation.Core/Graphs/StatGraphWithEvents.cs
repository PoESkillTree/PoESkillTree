using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class StatGraphWithEvents : IStatGraph
    {
        private readonly IStatGraph _decoratedGraph;
        private readonly Action<NodeType> _nodeAddedAction;
        private readonly Action<NodeType> _nodeRemovedAction;

        public StatGraphWithEvents(
            IStatGraph decoratedGraph, Action<NodeType> nodeAddedAction, Action<NodeType> nodeRemovedAction)
        {
            _decoratedGraph = decoratedGraph;
            _nodeAddedAction = nodeAddedAction;
            _nodeRemovedAction = nodeRemovedAction;
        }

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeType nodeType)
        {
            var nodeIsNew = !Nodes.ContainsKey(nodeType);
            var node = _decoratedGraph.GetNode(nodeType);
            if (nodeIsNew)
            {
                _nodeAddedAction(nodeType);
            }
            return node;
        }

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> Nodes =>
            _decoratedGraph.Nodes;

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(Form form) =>
            _decoratedGraph.GetFormNodeCollection(form);

        public IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _decoratedGraph.FormNodeCollections;

        public void RemoveNode(NodeType nodeType)
        {
            _decoratedGraph.RemoveNode(nodeType);
            _nodeRemovedAction(nodeType);
        }

        public void RemoveFormNodeCollection(Form form) => _decoratedGraph.RemoveFormNodeCollection(form);

        public void AddModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier) =>
            _decoratedGraph.AddModifier(node, modifier);

        public void RemoveModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier) =>
            _decoratedGraph.RemoveModifier(node, modifier);

        public int ModifierCount => _decoratedGraph.ModifierCount;
    }
}