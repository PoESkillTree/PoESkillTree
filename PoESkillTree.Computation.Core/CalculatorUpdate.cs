using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Parameter object for <see cref="ICalculator.Update"/>.
    /// </summary>
    public class CalculatorUpdate : ValueObject
    {
        public static CalculatorUpdate Empty { get; } = new CalculatorUpdate(new Modifier[0], new Modifier[0]);

        public CalculatorUpdate(
            IReadOnlyList<Modifier> addedModifiers,
            IReadOnlyList<Modifier> removedModifiers)
        {
            AddedModifiers = addedModifiers;
            RemovedModifiers = removedModifiers;
        }

        /// <summary>
        /// The modifiers added in this update.
        /// </summary>
        public IReadOnlyList<Modifier> AddedModifiers { get; }

        /// <summary>
        /// The modifiers removed in this update.
        /// </summary>
        public IReadOnlyList<Modifier> RemovedModifiers { get; }

        protected override object ToTuple()
            => (WithSequenceEquality(AddedModifiers), WithSequenceEquality(RemovedModifiers));

        public CalculatorUpdate Invert()
            => new CalculatorUpdate(RemovedModifiers, AddedModifiers);

        public static CalculatorUpdate Accumulate(CalculatorUpdate l, CalculatorUpdate r)
        {
            var added = new Dictionary<Modifier, List<Modifier>>();
            var removed = new Dictionary<Modifier, List<Modifier>>();
            RemoveOrAdd(l.RemovedModifiers, added, removed);
            RemoveOrAdd(l.AddedModifiers, removed, added);
            RemoveOrAdd(r.RemovedModifiers, added, removed);
            RemoveOrAdd(r.AddedModifiers, removed, added);
            return new CalculatorUpdate(added.Values.Flatten().ToList(), removed.Values.Flatten().ToList());
        }

        private static void RemoveOrAdd(IEnumerable<Modifier> modifiers,
            Dictionary<Modifier, List<Modifier>> removeFrom, Dictionary<Modifier, List<Modifier>> addTo)
        {
            foreach (var modifier in modifiers)
            {
                if (removeFrom.TryGetValue(modifier, out var list))
                    list.Remove(modifier);
                else
                    addTo.GetOrAdd(modifier, _ => new List<Modifier>()).Add(modifier);
            }
        }
    }
}