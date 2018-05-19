using System;

namespace PoESkillTree.Computation.Core.Events
{
    public static class SuspendableEventViewProvider
    {
        /// <summary>
        /// Creates an <see cref="ISuspendableEventViewProvider{T}"/> using <paramref name="defaultView"/> as
        /// <see cref="ISuspendableEventViewProvider{T}.DefaultView"/>, <paramref name="suspendableView"/> as
        /// <see cref="ISuspendableEventViewProvider{T}.SuspendableView"/> and
        /// <see cref="ISuspendableEventViewProvider{T}.Suspender"/>, and both to calculate
        /// <see cref="ICountsSubsribers.SubscriberCount"/>.
        /// </summary>
        public static ISuspendableEventViewProvider<T> Create<T, TSuspendable>(
            T defaultView, TSuspendable suspendableView)
            where T : ICountsSubsribers
            where TSuspendable : T, ISuspendableEvents
        {
            return new SuspendableEventViewProvider<T>(defaultView, suspendableView, suspendableView, 
                () => defaultView.SubscriberCount + suspendableView.SubscriberCount);
        }
    }


    /// <inheritdoc />
    /// <summary>
    /// Trivial implementation of <see cref="ISuspendableEventViewProvider{T}" /> that gets all the required parameters
    /// passed to it through the constructor.
    /// </summary>
    public class SuspendableEventViewProvider<T> : ISuspendableEventViewProvider<T>
    {
        private readonly Func<int> _countSubscribers;

        public SuspendableEventViewProvider(
            T defaultView, T suspendableView, ISuspendableEvents suspender, Func<int> countSubscribers)
        {
            _countSubscribers = countSubscribers;
            DefaultView = defaultView;
            SuspendableView = suspendableView;
            Suspender = suspender;
        }

        public T DefaultView { get; }
        public T SuspendableView { get; }
        public ISuspendableEvents Suspender { get; }
        public int SubscriberCount => _countSubscribers();
    }
}