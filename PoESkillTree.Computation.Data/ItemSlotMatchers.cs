using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Data
{
    public class ItemSlotMatchers : IReferencedMatchers<ItemSlot>
    {
        public string ReferenceName { get; } = nameof(ItemSlotMatchers);

        public IReadOnlyList<ReferencedMatcherData<ItemSlot>> Matchers { get; } = new[]
        {
            // Helmet in Hierophant and Helm in Ascendant's Hierophant ...
            ("helmet", ItemSlot.Helm),
            ("helm", ItemSlot.Helm),
            ("gloves", ItemSlot.Gloves),
            ("boots", ItemSlot.Boots),
        }.Select(t => new ReferencedMatcherData<ItemSlot>(t.Item1, t.Item2)).ToList();
    }
}