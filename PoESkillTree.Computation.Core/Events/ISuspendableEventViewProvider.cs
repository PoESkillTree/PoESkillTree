namespace PoESkillTree.Computation.Core.Events
{
    /// <summary>
    /// Provides two views onto objects of type <typeparamref name="T"/>: <see cref="DefaultView"/> will always raise
    /// events. <see cref="SuspendableView"/> will raise events subject to <see cref="Suspender"/>, i.e. they may be
    /// raised delayed.
    /// <para>
    /// The reason for this interface: The calculation graph itself has to propagate change events as they
    /// arise so it can invalidate its node values. But the interfaces surfaced by <see cref="ICalculator"/> should
    /// only send events at the end of <see cref="ICalculator.Update"/>, i.e. they have to be suspended while the
    /// update is in progress. To enable that, <see cref="DefaultView"/> is the instance used in the calculation
    /// itself and <see cref="SuspendableView"/> is the instance surfaced by <see cref="ICalculator"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Type of the views</typeparam>
    public interface ISuspendableEventViewProvider<out T> : ICountsSubsribers
    {
        /// <summary>
        /// Events raised by <see cref="DefaultView"/> are not subject to <see cref="Suspender"/>.
        /// <see cref="DefaultView"/> raises events as soon as they occur.
        /// </summary>
        T DefaultView { get; }

        /// <summary>
        /// Events raised by <see cref="SuspendableView"/> are subject to <see cref="Suspender"/>.
        /// <see cref="SuspendableView"/> does not raise events between <c>Suspender.SuspendEvents()</c> and
        /// <c>Suspender.ResumeEvents()</c> calls and will raise them once <c>Suspender.ResumeEvents()</c> is called.
        /// </summary>
        T SuspendableView { get; }

        /// <summary>
        /// The <see cref="ISuspendableEvents"/> used to suspends the events of <see cref="SuspendableView"/>.
        /// </summary>
        ISuspendableEvents Suspender { get; }
    }
}