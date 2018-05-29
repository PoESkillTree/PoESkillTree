namespace PoESkillTree.Computation.Core.Events
{
    /// <summary>
    /// Interface for (single-)event classes that need to expose the number of subscribers.
    /// </summary>
    public interface ICountsSubsribers
    {
        /// <summary>
        /// The number of subscribers to this class' event.
        /// </summary>
        int SubscriberCount { get; }
    }
}