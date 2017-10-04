namespace PoESkillTree.Computation.Parsing.Steps
{
    public sealed class SpecialStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.Special;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return new CompletedStep<ParsingStep, bool>(true, ParsingStep.Invalid);
            }
            return new StatManipulatorStep();
        }

        public override bool Equals(object obj)
        {
            return obj is SpecialStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}