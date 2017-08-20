namespace PoESkillTree.Computation.Parsing.Steps
{
    public class DamageStatStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.DamageStat;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return new ConditionStep();
            }
            return new PoolStatStep();
        }

        public override bool Equals(object obj)
        {
            return obj is DamageStatStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}