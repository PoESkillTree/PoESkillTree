namespace PoESkillTree.Computation.Builders.Skills
{
    public struct SkillDefinition
    {
        public SkillDefinition(string identifier, int numericId)
        {
            Identifier = identifier;
            NumericId = numericId;
        }

        public string Identifier { get; }
        public int NumericId { get; }
    }
}