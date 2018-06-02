using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PoESkillTree.Computation.Common.Tests
{
    [DebuggerDisplay("{" + nameof(_instance) + "}")]
    public class StatStub : IStat
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
        public Entity Entity => Entity.Character;
        public bool IsRegisteredExplicitly { get; set; }
        public Type DataType => typeof(double);
        public IEnumerable<Behavior> Behaviors => Enumerable.Empty<Behavior>();
    }
}