using JetBrains.Annotations;

namespace PoESkillTree.Computation
{
    public class ReferencedMatcherData<T>
    {
        public string Regex { get; }

        public T Match { get; }

        public ReferencedMatcherData([RegexPattern] string regex, T match)
        {
            Regex = regex;
            Match = match;
        }
    }
}