using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    internal class Stat : IStat
    {
        public bool Equals(IStat other) => Equals((object) other);

        public IStat Minimum { get; set; }
        public IStat Maximum { get; set; }
        public bool IsRegisteredExplicitly { get; set; }
        public Type DataType => typeof(double);
        public IEnumerable<Behavior> Behaviors { get; set; } = Enumerable.Empty<Behavior>();
    }
}