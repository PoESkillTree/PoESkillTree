using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    // Stats that are always given
    public interface IGivenStats
    {
        // Not processed, need matching
        IReadOnlyList<string> GivenStatLines { get; }

        // Already processed into providers
        IReadOnlyList<GivenStatData> GivenStats { get; }
    }
}