using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public static class CalculatorExtensions
    {
        /// <summary>
        /// Starts creating a new <see cref="CalculatorUpdate"/>. Can be submitted to <paramref name="graph"/>
        /// using <see cref="BatchUpdate.DoUpdate"/>.
        /// </summary>
        public static BatchUpdate NewBatchUpdate(this ICalculator graph) => 
            new BatchUpdate(graph, new Modifier[0], new Modifier[0]);


        public class BatchUpdate
        {
            private readonly ICalculator _graph;

            private readonly IReadOnlyList<Modifier> _added;
            private readonly IReadOnlyList<Modifier> _removed;

            public BatchUpdate(ICalculator graph, IReadOnlyList<Modifier> added, IReadOnlyList<Modifier> removed)
            {
                _graph = graph;
                _added = added;
                _removed = removed;
            }

            /// <summary>
            /// Returns new update that includes the given modifier to be added.
            /// </summary>
            public BatchUpdate AddModifier(Modifier modifier) => 
                AddModifiers(new[] { modifier });

            /// <summary>
            /// Returns new update that includes the given modifiers to be added.
            /// </summary>
            public BatchUpdate AddModifiers(IEnumerable<Modifier> added)
            {
                var newAdded = _added.Concat(added).ToList();
                return new BatchUpdate(_graph, newAdded, _removed);
            }

            /// <summary>
            /// Returns new update that includes the given modifier to be removed.
            /// </summary>
            public BatchUpdate RemoveModifier(Modifier modifier) => 
                RemoveModifiers(new[] { modifier });
            
            /// <summary>
            /// Returns new update that includes the given modifiers to be removed.
            /// </summary>
            public BatchUpdate RemoveModifiers(IEnumerable<Modifier> removed)
            {
                var newRemoved = _removed.Concat(removed).ToList();
                return new BatchUpdate(_graph, _added, newRemoved);
            }

            /// <summary>
            /// Calls <see cref="ICalculator.Update"/> with this update.
            /// </summary>
            public void DoUpdate()
            {
                _graph.Update(new CalculatorUpdate(_added, _removed));
            }
        }
    }
}