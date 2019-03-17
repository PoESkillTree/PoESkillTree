using System;

namespace PoESkillTree.ItemFilter.Model
{
    public class MatchItemLevel : MatchNumber
    {
        public MatchItemLevel(Operator op, int itemLevel)
            : base(op, itemLevel, 1, 100)
        {
            Keyword = "ItemLevel";
            Priority = Type.ItemLevel;
        }

        public MatchItemLevel(Operator op, int itemLevel, int itemLevel2)
            : base(op, itemLevel, itemLevel2, 1, 100)
        {
            Keyword = "ItemLevel";
            Priority = Type.ItemLevel;
        }
    }
}
