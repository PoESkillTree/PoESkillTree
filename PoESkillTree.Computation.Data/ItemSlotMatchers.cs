using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    public class ItemSlotMatchers : ReferencedMatchersBase<ItemSlot>
    {
        protected override IEnumerable<ReferencedMatcherData> CreateCollection() => new[]
        {
            // Helmet in Hierophant and Helm in Ascendant's Hierophant ...
            ("helmet", ItemSlot.Helm),
            ("helm", ItemSlot.Helm),
            ("gloves", ItemSlot.Gloves),
            ("boots", ItemSlot.Boots),
        }.Select(t => new ReferencedMatcherData(t.Item1, t.Item2));
    }
}