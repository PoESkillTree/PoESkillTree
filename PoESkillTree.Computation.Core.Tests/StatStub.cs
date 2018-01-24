using System.Diagnostics;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [DebuggerDisplay("{" + nameof(_instance) + "}")]
    internal class StatStub : IStat
    {
        private static int _instanceCounter;

        private readonly int _instance;

        public StatStub()
        {
            _instance = _instanceCounter++;
        }

        public bool Equals(IStat other) => Equals((object) other);

        public IStat Minimum => null;
        public IStat Maximum => null;
    }
}