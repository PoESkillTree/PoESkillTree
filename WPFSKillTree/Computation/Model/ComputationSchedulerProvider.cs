using System.Reactive.Concurrency;
using System.Threading;

namespace POESKillTree.Computation.Model
{
    public class ComputationSchedulerProvider
    {
        public IScheduler CalculationThread { get; }
            = new EventLoopScheduler(a => new Thread(a) { Name = "Calculation", IsBackground = true });

        public IScheduler Dispatcher { get; } = DispatcherScheduler.Current;
        public IScheduler TaskPool { get; } = TaskPoolScheduler.Default;
        public IScheduler NewThread { get; } = NewThreadScheduler.Default;
    }
}