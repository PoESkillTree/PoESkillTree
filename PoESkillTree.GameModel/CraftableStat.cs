using PoESkillTree.Utils;

namespace PoESkillTree.GameModel
{
    public class CraftableStat : ValueObject
    {
        public CraftableStat(string statId, int minValue, int maxValue)
            => (StatId, MinValue, MaxValue) = (statId, minValue, maxValue);

        public string StatId { get; }
        public int MinValue { get; }
        public int MaxValue { get; }

        protected override object ToTuple() => (StatId, MinValue, MaxValue);
    }
}