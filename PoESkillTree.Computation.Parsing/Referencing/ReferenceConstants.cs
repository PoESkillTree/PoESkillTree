using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public static class ReferenceConstants
    {
        public static readonly Regex ReferenceRegex = new Regex(@"\(\{(\w+)\}\)");
    }
}