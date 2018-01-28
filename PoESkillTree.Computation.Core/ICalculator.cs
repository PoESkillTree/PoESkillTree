using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Main interface of Computation.Core
    public interface ICalculator
    {
        // The order in which modifiers are added/removed may be changed to speed up the update process
        void Update(CalculatorUpdate update);

        INodeRepository NodeRepository { get; }

        IExternalStatRegistry ExternalStatRegistry { get; }
    }


    public class CalculatorUpdate
    {
        public CalculatorUpdate(
            IReadOnlyCollection<Modifier> addedModifiers, 
            IReadOnlyCollection<Modifier> removedModifiers)
        {
            AddedModifiers = addedModifiers;
            RemovedModifiers = removedModifiers;
        }

        public IReadOnlyCollection<Modifier> AddedModifiers { get; }
        public IReadOnlyCollection<Modifier> RemovedModifiers { get; }

        public override bool Equals(object obj) => 
            (this == obj) || (obj is CalculatorUpdate other && Equals(other));

        private bool Equals(CalculatorUpdate other) =>
            AddedModifiers.SequenceEqual(other.AddedModifiers)
            && RemovedModifiers.SequenceEqual(other.RemovedModifiers);

        public override int GetHashCode() => (AddedModifiers, RemovedModifiers).GetHashCode();
    }
}