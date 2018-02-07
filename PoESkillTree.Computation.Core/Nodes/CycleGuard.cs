using System;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class CycleGuard : ICycleGuard
    {
        private bool _guarded;

        public IDisposable Guard()
        {
            if (_guarded)
                throw new InvalidOperationException("Cyclic recursion detected");
            _guarded = true;
            return new Disposable(this);
        }


        private class Disposable : IDisposable
        {
            private readonly CycleGuard _cycleGuard;

            public Disposable(CycleGuard cycleGuard)
            {
                _cycleGuard = cycleGuard;
            }

            public void Dispose()
            {
                _cycleGuard._guarded = false;
            }
        }
    }
}