namespace PoESkillTree.Computation.Parsing.Steps
{
    public class PoolStatStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.PoolStat;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return new ConditionStep();
            }
            return new CompletedStep<ParsingStep, bool>(false, ParsingStep.Invalid);
        }

        public override bool Equals(object obj)
        {
            return obj is PoolStatStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}