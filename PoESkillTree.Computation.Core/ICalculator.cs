using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /* Main interface of Computation.Core
     *
     * Events exposed by this interface can easily be used by the UI. They are only raised at the end of Update().
     *
     * The interface can be used pull- or push-based: Call .Value on interesting nodes yourself after Update() or
     * only call it on nodes you are subscribed to when they raise events.
     *
     * Values for user specified conditions/stats can be set using modifiers with TotalOverride form
     * and read/subscribed to in the usual manner (UI needs to make sure writing and reading doesn't loop).
     * IStat.DataType, .Minimum and .Maximum can be used to determine how to display the input field.
     *
     * Something that might be counter-intuitive:
     * Don't store any node or collection you don't subscribe to. ICalculator can't know that you are still using
     * nodes you are not subscribed to, resulting in them being pruned from the calculation graph and you now holding
     * a disposed instance.
     * (if this turns into a usability problem, creating extension methods on the problematic interfaces, that subscribe
     * to the instance with no-ops and return IDisposable to unsubscribe when done using them, shouldn't be much work)
     *
     * If the delayed events turn out to be a performance issue in the "preview calculations" use case, they could
     * easily be disabled: Either a property on ICalculator that turns off suspender usage in Calculator or a different
     * factory method than/parameter to Calculator.CreateCalculator() that passes an empty suspender to the constructor.
     */
    public interface ICalculator
    {
        void Update(CalculatorUpdate update);

        INodeRepository NodeRepository { get; }

        INodeCollection<IStat> ExplicitlyRegisteredStats { get; }
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