using System.Diagnostics;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [DebuggerDisplay("{" + nameof(_instance) + "}")]
    internal class StatStub : IStat
    {
        private static int _instanceCounter;

        private readonly int _instance;

        public StatStub(IStat minimum = null, IStat maximum = null)
        {
            _instance = _instanceCounter++;
            Minimum = minimum;
            Maximum = maximum;
        }

        public bool Equals(IStat other) => Equals((object) other);

        public IStat Minimum { get; }
        public IStat Maximum { get; }
    }
}