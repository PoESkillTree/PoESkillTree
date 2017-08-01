using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Data
{
    public class ItemSlotMatchers : IReferencedMatchers<ItemSlot>
    {
        public IReadOnlyList<(string regex, ItemSlot match)> Matchers { get; } = new[]
        {
            // Helmet in Hierophant and Helm in Ascendant's Hierophant ...
            ("helmet", ItemSlot.Helm),
            ("helm", ItemSlot.Helm),
            ("gloves", ItemSlot.Gloves),
            ("boots", ItemSlot.Boots),
        };
    }
}