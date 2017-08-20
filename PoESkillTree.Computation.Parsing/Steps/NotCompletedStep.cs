namespace PoESkillTree.Computation.Parsing.Steps
{
    public abstract class NotCompletedStep<TStep, TData> : IStep<TStep, TData>
    {
        public bool Completed { get; } = false;
        public bool Successful { get; } = false;

        public abstract TStep Current { get; }
        public abstract IStep<TStep, TData> Next(TData data);
    }
}