namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IStepper<T>
    {
        T InitialStep { get; }

        T NextOnSuccess(T current);

        T NextOnFailure(T current);

        bool IsTerminal(T step);

        bool IsSuccess(T step);
    }
}