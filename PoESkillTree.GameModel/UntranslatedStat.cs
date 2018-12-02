namespace PoESkillTree.GameModel
{
    public class UntranslatedStat
    {
        public UntranslatedStat(string statId, int value) => (StatId, Value) = (statId, value);

        public string StatId { get; }
        public int Value { get; }

        public override bool Equals(object obj)
            => obj is UntranslatedStat other && Equals(other);

        private bool Equals(UntranslatedStat other)
            => StatId == other.StatId && Value == other.Value;

        public override int GetHashCode()
            => (StatId, Value).GetHashCode();

        public override string ToString()
            => $"{StatId}, {Value}";
    }
}