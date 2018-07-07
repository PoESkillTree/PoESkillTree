using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Main interface of Computation.Core. Represents a graph that calculates stat values based on modifiers.
    /// <para>
    /// An instance of this interface can be created with <see cref="Calculator.CreateCalculator"/>.
    /// </para>
    /// <para>
    /// All values of nodes are calculated lazily. <see cref="Update"/> only invalidates previously stored values
    /// on nodes that changed and raise re-calculation events. This also means that new nodes could be added
    /// when calculating values of other nodes.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Events exposed by this interface can easily be used by the UI. They are only raised at the end of
    /// <see cref="Update"/>.
    /// <para>
    /// The interface can be used pull- or push-based: Call <see cref="ICalculationNode.Value"/> on interesting nodes
    /// yourself after each <see cref="Update"/> or only call it on nodes you are subscribed to when they raise events.
    /// </para>
    /// <para>
    /// Nodes and collections returned by this interface should not be stored, unless you subscribe to their events.
    /// <see cref="ICalculator"/> can't know that they are still used when they are not subscribed to, resulting in
    /// them being pruned from the calculation graph and the stored instance being no longer valid.
    /// Instead, request them every time you need their value. (might make sense to add extension methods to
    /// INodeRepository that return the value instead of the node)
    /// </para>
    /// </remarks>
    /* If the delayed events turn out to be a performance issue in the "preview calculations" use case, they could
     * easily be disabled: Either a property on ICalculator that turns off suspender usage in Calculator or a different
     * factory method than/parameter to Calculator.CreateCalculator() that passes an empty suspender to the constructor.
     */
    public interface ICalculator
    {
        /// <summary>
        /// Updates the graph. Events of instances exposed through this interface will only be raised after everything
        /// was updated.
        /// <para>
        /// For easier usage, use <see cref="CalculatorExtensions.NewBatchUpdate"/>.
        /// </para>
        /// </summary>
        void Update(CalculatorUpdate update);

        /// <summary>
        /// The <see cref="INodeRepository"/> that can be used to retrieve the values of nodes.
        /// </summary>
        INodeRepository NodeRepository { get; }

        /// <summary>
        /// Collection of all <see cref="IStat"/>s and their (<see cref="NodeType.Total"/>) nodes that are part of
        /// the calculation graph and have a <see cref="IStat.ExplicitRegistrationType"/> value. These
        /// are stats that need their values to be set explicitly by users.
        /// </summary>
        /// <remarks>
        /// Values for these stats can be set using modifiers with <see cref="Form.TotalOverride"/> and read/subscribed
        /// to in the usual manner (UI needs to make sure writing and reading doesn't loop). When changing the value
        /// by adding a new modifier, you need to also remove the previous modifier. You could bind the modifier's
        /// <see cref="IValue"/> to your UI, but then the <see cref="ICalculator"/> can't know when the value changes
        /// and needs to be re-calculated.
        /// <para>
        /// <see cref="IStat.DataType"/>, <see cref="IStat.Minimum"/> and <see cref="IStat.Maximum"/> can be used to
        /// determine how to display the input.
        /// </para>
        /// </remarks>
        INodeCollection<IStat> ExplicitlyRegisteredStats { get; }
    }


    /// <summary>
    /// Parameter object for <see cref="ICalculator.Update"/>.
    /// </summary>
    public class CalculatorUpdate
    {
        public CalculatorUpdate(
            IReadOnlyCollection<Modifier> addedModifiers,
            IReadOnlyCollection<Modifier> removedModifiers)
        {
            AddedModifiers = addedModifiers;
            RemovedModifiers = removedModifiers;
        }

        /// <summary>
        /// The modifiers added in this update.
        /// </summary>
        public IReadOnlyCollection<Modifier> AddedModifiers { get; }

        /// <summary>
        /// The modifiers removed in this update.
        /// </summary>
        public IReadOnlyCollection<Modifier> RemovedModifiers { get; }

        public override bool Equals(object obj) =>
            (this == obj) || (obj is CalculatorUpdate other && Equals(other));

        private bool Equals(CalculatorUpdate other) =>
            AddedModifiers.SequenceEqual(other.AddedModifiers)
            && RemovedModifiers.SequenceEqual(other.RemovedModifiers);

        public override int GetHashCode() =>
            (AddedModifiers.SequenceHash(), RemovedModifiers.SequenceHash()).GetHashCode();
    }
}