using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
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