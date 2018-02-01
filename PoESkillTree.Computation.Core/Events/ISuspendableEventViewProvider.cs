namespace PoESkillTree.Computation.Core.Events
{
    public interface ISuspendableEventViewProvider<out T> : ICountsSubsribers
    {
        // Events raised by T are not subject to Suspender. T raises events as they are received.
        T DefaultView { get; }

        // Events raised by T are subject to Suspender. T may raise events with a delay.
        T SuspendableView { get; }

        ISuspendableEvents Suspender { get; }
    }
}