using System;
using System.Collections.Generic;
using System.Diagnostics;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    [DebuggerDisplay("{" + nameof(Identity) + "}")]
    internal class Stat : IStat
    {
        public Stat(string name = "") => Identity = name;

        public bool Equals(IStat other) => Equals((object) other);

        public IStat Minimum { get; set; }
        public IStat Maximum { get; set; }
        public string Identity { get; }
        public Entity Entity => Entity.Character;
        public bool IsRegisteredExplicitly { get; set; }
        public Type DataType => typeof(double);
        public IReadOnlyCollection<Behavior> Behaviors { get; set; } = new Behavior[0];
    }
}