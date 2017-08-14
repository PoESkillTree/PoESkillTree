namespace PoESkillTree.Computation
{
    public interface IParser<TResult>
    {
        // Returns true and outputs a result if the stat could be parsed
        // If it returns false, result may or may not be non-null (should only be used for debugging)
        bool TryParse(string stat, out TResult result);
    }

    /*
     * - Root parser IParser<IMatch> is called by computation
     * - Calls IParser<(string remainingStat, IMatchBuilder matchBuilder)> instances in an order
     *   that is defined in some strategy class
     * - Returned IMatchBuilders are combined in some way into one
     * - The combined IMatchBuilder is built to an IMatch instance
     */
}