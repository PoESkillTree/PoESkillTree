namespace PoESkillTree.Computation.Parsing.Steps
{
    public class ConditionStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.Condition;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return this;
            }
            return new CompletedStep<ParsingStep, bool>(true, ParsingStep.Invalid);
        }

        public override bool Equals(object obj)
        {
            return obj is ConditionStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}