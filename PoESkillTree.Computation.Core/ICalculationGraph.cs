using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Main interface of Computation.Core (better name? clients don't care that it's a graph)
    // Two implementations:
    // - For push-based usage: Two-pass update as written down in ICalculationNode.cs. Returned nodes are CachingNodes.
    // - For pull-based usage: Single-pass update (no suspend/resume). Returned nodes are CachningNodeAdapters.
    public interface ICalculationGraph
    {
        // The order in which modifiers are added/removed may be changed to speed up the update process
        void Update(CalculationGraphUpdate update);

        INodeRepository NodeRepository { get; }

        IExternalStatRegistry ExternalStatRegistry { get; }
    }


    public class CalculationGraphUpdate
    {
        public CalculationGraphUpdate(
            IReadOnlyCollection<Modifier> addedModifiers, 
            IReadOnlyCollection<Modifier> removedModifiers)
        {
            AddedModifiers = addedModifiers;
            RemovedModifiers = removedModifiers;
        }

        public IReadOnlyCollection<Modifier> AddedModifiers { get; }
        public IReadOnlyCollection<Modifier> RemovedModifiers { get; }

        public override bool Equals(object obj) => 
            (this == obj) || (obj is CalculationGraphUpdate other && Equals(other));

        private bool Equals(CalculationGraphUpdate other) =>
            AddedModifiers.SequenceEqual(other.AddedModifiers)
            && RemovedModifiers.SequenceEqual(other.RemovedModifiers);

        public override int GetHashCode() => (AddedModifiers, RemovedModifiers).GetHashCode();
    }
}