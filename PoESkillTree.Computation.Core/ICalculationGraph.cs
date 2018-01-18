using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Main interface of Computation.Core
    // Two implementations:
    // - For push-based usage: Two-pass update as written down in ICalculationNode.cs. Returned nodes are CachingNodes.
    // - For pull-based usage: Single-pass update (only first step of two-pass). Returned nodes CachningNodeAdapters.
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
            IReadOnlyCollection<(Modifier modifier, IModifierSource source)> addedModifiers, 
            IReadOnlyCollection<(Modifier modifier, IModifierSource source)> removedModifiers)
        {
            AddedModifiers = addedModifiers;
            RemovedModifiers = removedModifiers;
        }

        public IReadOnlyCollection<(Modifier modifier, IModifierSource source)> AddedModifiers { get; }
        public IReadOnlyCollection<(Modifier modifier, IModifierSource source)> RemovedModifiers { get; }

        private bool Equals(CalculationGraphUpdate other)
        {
            return AddedModifiers.SequenceEqual(other.AddedModifiers)
                   && RemovedModifiers.SequenceEqual(other.RemovedModifiers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CalculationGraphUpdate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (AddedModifiers.GetHashCode() * 397) ^ RemovedModifiers.GetHashCode();
            }
        }
    }
}