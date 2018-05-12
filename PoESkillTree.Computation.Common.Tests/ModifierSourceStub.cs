using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;

namespace PoESkillTree.Computation.Common.Tests
{
    [DebuggerDisplay("{" + nameof(_instance) + "}")]
    public class ModifierSourceStub : IModifierSource
    {
        private static int _instanceCounter;

        private readonly int _instance;

        public ModifierSourceStub(params IModifierSource[] influencingSources)
        {
            _instance = _instanceCounter++;
            InfluencingSources = this.Concat(influencingSources).ToList();
            CanonicalSource = this;
        }

        public bool Equals(IModifierSource other) => Equals((object) other);

        public ModifierSourceFirstLevel FirstLevel { get; set; } = ModifierSourceFirstLevel.Global;
        public IReadOnlyList<IModifierSource> InfluencingSources { get; }
        public IModifierSource CanonicalSource { get; set; }
    }
}