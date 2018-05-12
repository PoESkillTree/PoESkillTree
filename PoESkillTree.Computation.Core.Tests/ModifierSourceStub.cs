using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [DebuggerDisplay("{" + nameof(_instance) + "}")]
    internal class ModifierSourceStub : IModifierSource
    {
        private static int _instanceCounter;

        private readonly int _instance;

        public ModifierSourceStub(params IModifierSource[] influencingSources)
        {
            _instance = _instanceCounter++;
            InfluencingSources = this.Concat(influencingSources).ToList();
        }

        public bool Equals(IModifierSource other) => Equals((object) other);

        public ModifierSourceFirstLevel FirstLevel => ModifierSourceFirstLevel.Global;
        public IReadOnlyList<IModifierSource> InfluencingSources { get; }
        public IModifierSource ToCanonical() => this;
    }
}