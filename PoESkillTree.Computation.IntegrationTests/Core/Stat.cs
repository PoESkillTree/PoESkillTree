using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    [DebuggerDisplay("{" + nameof(_name) + "}")]
    internal class Stat : IStat
    {
        private readonly string _name;

        public Stat(string name = "") => _name = name;

        public bool Equals(IStat other) => Equals((object) other);

        public IStat Minimum { get; set; }
        public IStat Maximum { get; set; }
        public Entity Entity => Entity.Character;
        public bool IsRegisteredExplicitly { get; set; }
        public Type DataType => typeof(double);
        public IEnumerable<Behavior> Behaviors { get; set; } = Enumerable.Empty<Behavior>();
    }
}