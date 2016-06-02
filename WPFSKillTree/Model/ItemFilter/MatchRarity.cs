using System;

namespace POESKillTree.Model.ItemFilter
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

        private static readonly string[] Values =
        {
                "Normal",
                "Magic",
                "Rare",
                "Unique"
        };

        public MatchRarity(Operator op, Rarity rarity)
            : base(op, Values[(int)rarity])
        {
            Keyword = "Rarity";
            Priority = Type.Rarity;
        }
    }
}
