namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    // Returned instances need the matched mod line as context
    public interface IMatchContext<out T>
    {
        T this[int index] { get; }

        T First { get; }

        T Last { get; }

        // returns the only element or throws (on validation)
        T Single { get; }
    }
}