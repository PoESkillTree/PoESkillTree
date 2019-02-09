using System;
using System.Reactive.Disposables;

namespace POESKillTree.Utils
{
    /// <summary>
    /// A simple monitor that can be used to track method calls. Can be used to stop events from being handled
    /// while data is still being changed, for example.
    /// </summary>
    public class SimpleMonitor
    {
        private int _count;

        /// <summary>
        /// Returns true if at least one <see cref="Enter"/> call was not yet disposed.
        /// </summary>
        public bool IsBusy => _count > 0;

        /// <summary>
        /// Enters the monitor. Dispose the returned IDisposable to leave it.
        /// </summary>
        public IDisposable Enter()
        {
            if (_count == 0)
                Entered?.Invoke(this, EventArgs.Empty);
            _count++;
            return Disposable.Create(DisposeCallback);
        }

        private void DisposeCallback()
        {
            _count--;
            if (_count == 0)
                Freed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that is invoked every time <see cref="Enter"/> is called when <see cref="IsBusy"/> is false.
        /// </summary>
        public event EventHandler Entered;

        /// <summary>
        /// Event that is invoked every time all <see cref="Enter"/> calls are disposed.
        /// </summary>
        public event EventHandler Freed;
    }
}