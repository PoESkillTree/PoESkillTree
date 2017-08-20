namespace PoESkillTree.Computation.Parsing.Steps
{
    public class FormAndStatStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.FormAndStat;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return new ConditionStep();
            }
            return new FormStep();
        }

        public override bool Equals(object obj)
        {
            return obj is FormAndStatStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}