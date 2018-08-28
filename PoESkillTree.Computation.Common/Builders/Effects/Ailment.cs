using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Builders.Effects
{
    public enum Ailment
    {
        Ignite,
        Shock,
        Chill,
        Freeze,
        Bleed,
        Poison
    }

    public static class AilmentConstants
    {
        public static readonly IReadOnlyList<Ailment> DamagingAilments =
            new[] { Ailment.Ignite, Ailment.Bleed, Ailment.Poison };
    }
}