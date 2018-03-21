using System.Collections.Generic;

namespace PoESkillTree.Computation.Common
{
    public interface IModifierSource
    {
        // First level: Global, Local, Ailment
        // Global and Local:
        // - Second level: Given, Tree, Skill or Item (maybe more).
        // - Third level: item slot for items
        // - For global, further levels are only for more detailed breakdowns. For calculation, all global sources are
        //   considered the same.
        // - Also contains information about e.g. tree node names, "Dexterity", item names, ...
        // Ailment second level: Poison, Bleed, Ignite
        // (if necessary, these could inherit further levels from the Hit type damage they originate from)

        // 0th level: Any (for querying paths without specifying a modifier source)
        // Specifying higher levels is optional.
        // Sources can be merged together resulting in a source with the highest common level (requires 0th level).

        // The modifier sources of increase/more modifiers influence a base value of this source (including this source itself)
        // E.g.:
        // - Global: only Global
        // - Local->Item->BodyArmour: Local->Item->BodyArmour, Global, (Local->Item if such modifiers exist)
        IEnumerable<IModifierSource> InfluencingSources { get; }

        // Returns an instance that only contains data necessary for determining equivalence and no additional infos.
        // Such instances are what's stored in stat graph paths.
        IModifierSource ToCanonical();
    }
}