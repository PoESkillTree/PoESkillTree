using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation for <see cref="IItemSlotBuilder"/>s.
    /// </summary>
    public class ItemSlotMatchers : ReferencedMatchersBase<IItemSlotBuilder>
    {
        private readonly IItemSlotBuilders _itemSlotBuilders;

        public ItemSlotMatchers(IItemSlotBuilders itemSlotBuilders)
        {
            _itemSlotBuilders = itemSlotBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection() => new[]
        {
            // Helmet in Hierophant and Helm in Ascendant's Hierophant ...
            ("helmet", ItemSlot.Helm),
            ("helm", ItemSlot.Helm),
            ("gloves", ItemSlot.Gloves),
            ("boots", ItemSlot.Boots),
        }.Select(t => new ReferencedMatcherData(t.Item1, _itemSlotBuilders.From(t.Item2)));
    }
}