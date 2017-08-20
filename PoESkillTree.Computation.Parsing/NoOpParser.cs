namespace PoESkillTree.Computation.Parsing
{
    public class NoOpParser<TResult> : IParser<TResult>
    {
        public bool TryParse(string stat, out string remaining, out TResult result)
        {
            remaining = stat;
            result = default(TResult);
            return false;
        }
    }
}