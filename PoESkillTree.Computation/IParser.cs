namespace PoESkillTree.Computation
{
    public interface IParser<TResult>
    {
        // Returns true and outputs a result if the stat could be parsed
        // If it returns false, result may or may not be non-null (should only be used for debugging)
        // remaining are the parts of stat that were not parsed into the result. If false is
        // returned and result is null, remaining == stat.
        bool TryParse(string stat, out string remaining, out TResult result);
    }
}