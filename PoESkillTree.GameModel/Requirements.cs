namespace PoESkillTree.GameModel
{
    public class Requirements
    {
        public Requirements(int level, int dexterity, int intelligence, int strength)
            => (Level, Dexterity, Intelligence, Strength) = (level, dexterity, intelligence, strength);

        public int Level { get; }

        public int Dexterity { get; }
        public int Intelligence { get; }
        public int Strength { get; }
    }
}