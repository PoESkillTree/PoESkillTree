namespace PoESkillTree.Computation.Parsing.Steps
{
    public class GeneralStatStep : NotCompletedStep<ParsingStep, bool>
    {
        public override ParsingStep Current { get; } = ParsingStep.GeneralStat;

        public override IStep<ParsingStep, bool> Next(bool data)
        {
            if (data)
            {
                return new ConditionStep();
            }
            return new DamageStatStep();
        }

        public override bool Equals(object obj)
        {
            return obj is GeneralStatStep;
        }

        public override int GetHashCode()
        {
            return (int) Current;
        }
    }
}