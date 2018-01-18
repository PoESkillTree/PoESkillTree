using System.Collections.Generic;
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

    
    // The most convenient solution to create these would probably be an extension method on ICalculationGraph
    // ("StartBatchUpdate" or something), returning a class with AddModifier(), RemoveModifier(), Swap() and
    // Commit() methods.
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
    }
}