using System;

namespace POESKillTree.ItemFilter.Model
{
    public class MatchRarity : MatchEnum
    {
        public enum Rarity
        {
            Normal,
            Magic,
            Rare,
            Unique
        }

        private static readonly string[] RarityValues = { "Normal", "Magic", "Rare", "Unique" };

        // Implicit match.
        public MatchRarity()
            : base(MatchEnum.Operator.GreaterOrEqual, (int)Rarity.Normal, RarityValues)
        {
            Keyword = "Rarity";
            Priority = Type.Rarity;
        }

        public MatchRarity(Operator op, Rarity rarity)
            : base(op, (int)rarity, RarityValues)
        {
            Keyword = "Rarity";
            Priority = Type.Rarity;
        }
    }
}
