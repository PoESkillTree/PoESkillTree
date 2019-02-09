using System.Reactive.Concurrency;

namespace POESKillTree.Computation.Model
{
    public class ComputationSchedulerProvider
    {
        public ComputationSchedulerProvider()
            => Dispatcher = DispatcherScheduler.Current;

        public IScheduler CalculationThread { get; } = new EventLoopScheduler();
        public IScheduler Dispatcher { get; }
        public IScheduler TaskPool { get; } = TaskPoolScheduler.Default;
    }
}