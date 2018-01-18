using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public static class CalculationGraphExtensions
    {
        public static BatchUpdate NewBatchUpdate(this ICalculationGraph graph)
        {
            return new BatchUpdate(graph, 
                new (Modifier modifier, IModifierSource source)[0], new (Modifier modifier, IModifierSource source)[0]);
        }


        public class BatchUpdate
        {
            private readonly ICalculationGraph _graph;

            private readonly IReadOnlyList<(Modifier modifier, IModifierSource source)> _added;
            private readonly IReadOnlyList<(Modifier modifier, IModifierSource source)> _removed;

            public BatchUpdate(
                ICalculationGraph graph,
                IReadOnlyList<(Modifier modifier, IModifierSource source)> added,
                IReadOnlyList<(Modifier modifier, IModifierSource source)> removed)
            {
                _graph = graph;
                _added = added;
                _removed = removed;
            }

            public BatchUpdate AddModifier(Modifier modifier, IModifierSource source)
            {
                return AddModifiers(new[] { (modifier, source) });
            }

            public BatchUpdate AddModifiers(IEnumerable<(Modifier modifier, IModifierSource source)> added)
            {
                var newAdded = _added.Concat(added).ToList();
                return new BatchUpdate(_graph, newAdded, _removed);
            }

            public BatchUpdate RemoveModifier(Modifier modifier, IModifierSource source)
            {
                return RemoveModifiers(new[] { (modifier, source) });
            }

            public BatchUpdate RemoveModifiers(IEnumerable<(Modifier modifier, IModifierSource source)> removed)
            {
                var newRemoved = _removed.Concat(removed).ToList();
                return new BatchUpdate(_graph, _added, newRemoved);
            }

            public void DoUpdate()
            {
                _graph.Update(new CalculationGraphUpdate(_added, _removed));
            }
        }
    }
}