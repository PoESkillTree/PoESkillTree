namespace PoESkillTree.Computation.Core.Events
{
    /// <summary>
    /// Interface for event classes that support suspending and resuming raising their events.
    /// </summary>
    public interface ISuspendableEvents
    {
        /// <summary>
        /// Suspends raising of events. No events will be raised until <see cref="ResumeEvents"/> is called.
        /// </summary>
        void SuspendEvents();

        /// <summary>
        /// Resumes raising of events. Raises events that were suppressed after <see cref="SuspendEvents"/> was called.
        /// Those events will be combined into as few events as makes sense, e.g. a parameterless event will be raised
        /// at most once when calling this method.
        /// </summary>
        void ResumeEvents();
    }
}