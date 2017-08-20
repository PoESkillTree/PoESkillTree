namespace PoESkillTree.Computation.Parsing.Steps
{
    public sealed class StatManipulationStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.StatManipulation;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            return new ValueConversionStep();
        }

        public override bool Equals(object obj)
        {
            return obj is StatManipulationStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}