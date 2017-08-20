namespace PoESkillTree.Computation.Parsing.Steps
{
    public class ValueConversionStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.ValueConversion;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            return new FormAndStatStep();
        }

        public override bool Equals(object obj)
        {
            return obj is ValueConversionStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}