namespace PoESkillTree.Computation.Core
{
    public static class SuspendableEventViewProvider
    {
        public static ISuspendableEventViewProvider<T> Create<T, TSuspendable>(
            T defaultView, TSuspendable suspendableView)
            where TSuspendable: T, ISuspendableEvents
        {
            return new SuspendableEventViewProvider<T>(defaultView, suspendableView, suspendableView);
        }
    }


    public class SuspendableEventViewProvider<T> : ISuspendableEventViewProvider<T>
    {
        public SuspendableEventViewProvider(T defaultView, T suspendableView, ISuspendableEvents suspender)
        {
            DefaultView = defaultView;
            SuspendableView = suspendableView;
            Suspender = suspender;
        }

        public T DefaultView { get; }
        public T SuspendableView { get; }
        public ISuspendableEvents Suspender { get; }
    }
}