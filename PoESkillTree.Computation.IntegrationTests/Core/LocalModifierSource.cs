using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    internal class LocalModifierSource : IModifierSource
    {
        public LocalModifierSource()
        {
            InfluencingSources = new IModifierSource[] { this, new GlobalModifierSource(), };
        }

        public bool Equals(IModifierSource other) => Equals((object) other);

        public ModifierSourceFirstLevel FirstLevel => ModifierSourceFirstLevel.Local;
        public IReadOnlyList<IModifierSource> InfluencingSources { get; }
        public IModifierSource CanonicalSource => this;
    }
}