using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// Main interface of Computation.Core. Represents a graph that calculates stat values based on modifiers.
    /// <para>
    /// An instance of this interface can be created with <see cref="Calculator.Create"/>.
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
     * easily be disabled: Use ImmediateEventBuffer
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
        /// Removes unused nodes. Is done with every <see cref="Update"/> call but might be necessary after calculating
        /// node values. Nodes only become removable after the nodes using them were recalculated.
        /// </summary>
        void RemoveUnusedNodes();

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
}