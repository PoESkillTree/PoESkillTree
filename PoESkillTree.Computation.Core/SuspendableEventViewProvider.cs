using System;

namespace PoESkillTree.Computation.Core
{
    public static class SuspendableEventViewProvider
    {
        public static ISuspendableEventViewProvider<T> Create<T, TSuspendable>(
            T defaultView, TSuspendable suspendableView)
            where T : ICountsSubsribers
            where TSuspendable : T, ISuspendableEvents
        {
            return new SuspendableEventViewProvider<T>(defaultView, suspendableView, suspendableView, 
                () => defaultView.SubscriberCount + suspendableView.SubscriberCount);
        }
    }


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