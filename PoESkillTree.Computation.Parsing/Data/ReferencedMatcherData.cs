using System.Diagnostics;

namespace PoESkillTree.Computation.Parsing.Data
{
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class ReferencedMatcherData
    {
        public string Regex { get; }

        public object Match { get; }

        public ReferencedMatcherData(string regex, object match)
        {
            Regex = regex;
            Match = match;
        }
    }
}