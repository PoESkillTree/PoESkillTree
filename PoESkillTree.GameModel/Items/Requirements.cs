namespace PoESkillTree.GameModel.Items
{
    public class Requirements
    {
        public Requirements(int level, int dexterity, int intelligence, int strength)
            => (Level, Dexterity, Intelligence, Strength) = (level, dexterity, intelligence, strength);

        public int Level { get; }

        public int Dexterity { get; }
        public int Intelligence { get; }
        public int Strength { get; }

        public override bool Equals(object obj)
            => obj == this || (obj is Requirements other && Equals(other));

        private bool Equals(Requirements other)
            => ToTuple() == other.ToTuple();

        public override int GetHashCode()
            => ToTuple().GetHashCode();

        public override string ToString()
            => ToTuple().ToString();

        private (int level, int dexterity, int intelligence, int strength) ToTuple()
            => (Level, Dexterity, Intelligence, Strength);
    }
}