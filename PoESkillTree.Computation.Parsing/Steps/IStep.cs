namespace PoESkillTree.Computation.Parsing.Steps
{
    public interface IStep<out TStep, in TData>
    {
        bool Completed { get; }

        bool Successful { get; }

        TStep Current { get; }

        IStep<TStep, TData> Next(TData data);
    }
}