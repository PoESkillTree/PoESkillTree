using JetBrains.Annotations;

namespace PoESkillTree.Computation
{
    public class MatcherData
    {
        public string Regex { get; }

        public IMatchBuilder MatchBuilder { get; }

        public MatcherData([RegexPattern] string regex, IMatchBuilder matchBuilder)
        {
            Regex = regex;
            MatchBuilder = matchBuilder;
        }
    }
}