using System;

namespace POESKillTree.Utils
{
    /// <summary>
    /// A simple monitor that can be used to track method calls. Can be used to stop events from being handled
    /// while data is still being changed, for example.
    /// </summary>
    public class SimpleMonitor : IDisposable
    {
        private int _count;

        /// <summary>
        /// Returns true if at least one <see cref="Enter"/> call was not yet disposed.
        /// </summary>
        public bool IsBusy
        {
            get { return _count > 0; }
        }

        /// <summary>
        /// Enters the monitor. Dispose the returned IDisposable to leave it.
        /// </summary>
        public IDisposable Enter()
        {
            _count++;
            return this;
        }

        // Explicit override to force one dispose per enter.
        void IDisposable.Dispose()
        {
            _count--;
            if (_count == 0)
                Freed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that is invoked every time all <see cref="Enter"/> calls are disposed.
        /// </summary>
        public event EventHandler Freed;
    }
}