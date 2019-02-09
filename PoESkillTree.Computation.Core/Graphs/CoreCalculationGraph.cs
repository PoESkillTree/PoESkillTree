using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Core implementation of <see cref="ICalculationGraph"/>.
    /// </summary>
    public class CoreCalculationGraph : ICalculationGraph
    {
        private readonly Func<IStat, IStatGraph> _statGraphFactory;
        private readonly INodeFactory _nodeFactory;
        private readonly Dictionary<IStat, IStatGraph> _statGraphs = new Dictionary<IStat, IStatGraph>();

        private readonly Dictionary<Modifier, Stack<IDisposableNodeViewProvider>> _modifierNodes
            = new Dictionary<Modifier, Stack<IDisposableNodeViewProvider>>();

        public CoreCalculationGraph(Func<IStat, IStatGraph> statGraphFactory, INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
            _statGraphFactory = statGraphFactory;
        }

        public IEnumerator<IReadOnlyStatGraph> GetEnumerator() =>
            StatGraphs.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IStatGraph GetOrAddStatGraph(IStat stat) =>
            _statGraphs.GetOrAdd(stat, _statGraphFactory);

        public IReadOnlyStatGraph GetOrAdd(IStat stat) => GetOrAddStatGraph(stat);

        public IReadOnlyDictionary<IStat, IStatGraph> StatGraphs => _statGraphs;

        public void Remove(IStat stat) => _statGraphs.Remove(stat);

        public void AddModifier(Modifier modifier)
        {
            var node = _nodeFactory.Create(modifier.Value, new PathDefinition(modifier.Source.CanonicalSource));
            modifier.Stats
                .Select(GetOrAddStatGraph)
                .ForEach(g => g.AddModifier(node, modifier));
            _modifierNodes.GetOrAdd(modifier, k => new Stack<IDisposableNodeViewProvider>())
                .Push(node);
        }

        public void RemoveModifier(Modifier modifier)
        {
            if (!TryGetNodeProvider(modifier, out var node))
            {
                return;
            }

            modifier.Stats
                .Where(s => StatGraphs.ContainsKey(s))
                .Select(s => StatGraphs[s])
                .ForEach(g => g.RemoveModifier(node, modifier));

            node.Dispose();
        }

        private bool TryGetNodeProvider(
            Modifier modifier, out IDisposableNodeViewProvider nodeProvider)
        {
            if (!_modifierNodes.TryGetValue(modifier, out var stack))
            {
                nodeProvider = null;
                return false;
            }

            nodeProvider = stack.Pop();
            if (stack.IsEmpty())
            {
                _modifierNodes.Remove(modifier);
            }
            return true;
        }
    }
}