namespace PoESkillTree.Computation.Parsing.Steps
{
    public sealed class StatManipulatorStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.StatManipulator;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            return new ValueConversionStep();
        }

        public override bool Equals(object obj)
        {
            return obj is StatManipulatorStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}