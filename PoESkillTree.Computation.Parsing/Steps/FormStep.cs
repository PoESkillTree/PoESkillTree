namespace PoESkillTree.Computation.Parsing.Steps
{
    public class FormStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.Form;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return new GeneralStatStep();
            }
            return new CompletedStep<ParsingStep, bool>(false, ParsingStep.Invalid);
        }

        public override bool Equals(object obj)
        {
            return obj is FormStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}