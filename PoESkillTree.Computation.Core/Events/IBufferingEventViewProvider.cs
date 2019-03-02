namespace PoESkillTree.Computation.Core.Events
{
    /// <summary>
    /// Provides two views onto objects of type <typeparamref name="T"/>: <see cref="DefaultView"/> will always raise
    /// events. <see cref="BufferingView"/> will raise events subject through <see cref="IEventBuffer"/>, i.e. they may
    /// be buffered and raised delayed.
    /// <para>
    /// The reason for this interface: The calculation graph itself has to propagate change events as they
    /// arise so it can invalidate its node values. But the interfaces surfaced by <see cref="ICalculator"/> should
    /// only send events at the end of <see cref="ICalculator.Update"/>, i.e. they have to be suspended while the
    /// update is in progress. To enable that, <see cref="DefaultView"/> is the instance used in the calculation
    /// itself and <see cref="BufferingView"/> is the instance surfaced by <see cref="ICalculator"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Type of the views</typeparam>
    public interface IBufferingEventViewProvider<out T> : ICountsSubsribers
    {
        /// <summary>
        /// Events raised by <see cref="DefaultView"/> are not buffered.
        /// <see cref="DefaultView"/> raises events as soon as they occur.
        /// </summary>
        T DefaultView { get; }

        /// <summary>
        /// Events raised by <see cref="BufferingView"/> are buffered by <see cref="IEventBuffer"/> and may be delayed.
        /// </summary>
        T BufferingView { get; }
    }
}