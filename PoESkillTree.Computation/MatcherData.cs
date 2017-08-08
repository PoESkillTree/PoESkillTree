namespace PoESkillTree.Computation
{
    public class MatcherData
    {
        public string Regex { get; }

        public IMatchBuilder MatchBuilder { get; }

        public string MatchSubstitution { get; }

        public MatcherData(string regex, IMatchBuilder matchBuilder, string matchSubstitution = "")
        {
            Regex = regex;
            MatchBuilder = matchBuilder;
            MatchSubstitution = matchSubstitution;
        }
    }
}