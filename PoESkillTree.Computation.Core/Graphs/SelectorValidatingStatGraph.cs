using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class SelectorValidatingStatGraph : IStatGraph
    {
        private static readonly NodeType[] MainPathOnlyNodeTypes =
            { NodeType.Total, NodeType.Subtotal, NodeType.UncappedSubtotal, NodeType.TotalOverride };

        private static readonly Form[] MainPathOnlyForms = { Form.TotalOverride };

        private readonly IStatGraph _decoratedGraph;

        public SelectorValidatingStatGraph(IStatGraph decoratedGraph) =>
            _decoratedGraph = decoratedGraph;

        public ISuspendableEventViewProvider<IObservableCollection<PathDefinition>> Paths =>
            _decoratedGraph.Paths;

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeSelector selector)
        {
            if (!selector.Path.IsMainPath && MainPathOnlyNodeTypes.Contains(selector.NodeType))
                throw new ArgumentException(
                    $"{selector.NodeType} is only allowed with the main path", nameof(selector));

            return _decoratedGraph.GetNode(selector);
        }

        public IReadOnlyDictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>> Nodes =>
            _decoratedGraph.Nodes;

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(FormNodeSelector selector)
        {
            if (!selector.Path.IsMainPath && MainPathOnlyForms.Contains(selector.Form))
                throw new ArgumentException($"{selector.Form} is only allowed with the main path", nameof(selector));

            return _decoratedGraph.GetFormNodeCollection(selector);
        }

        public IReadOnlyDictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _decoratedGraph.FormNodeCollections;

        public void RemoveNode(NodeSelector selector) =>
            _decoratedGraph.RemoveNode(selector);

        public void RemoveFormNodeCollection(FormNodeSelector selector) =>
            _decoratedGraph.RemoveFormNodeCollection(selector);

        public void AddModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier) =>
            _decoratedGraph.AddModifier(node, modifier);

        public void RemoveModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier) =>
            _decoratedGraph.RemoveModifier(node, modifier);

        public int ModifierCount =>
            _decoratedGraph.ModifierCount;
    }
}