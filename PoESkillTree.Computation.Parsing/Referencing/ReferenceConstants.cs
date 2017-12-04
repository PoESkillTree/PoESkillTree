using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public static class ReferenceConstants
    {
        public const string ValuePlaceholder = "#";
        public static readonly Regex ValuePlaceholderRegex = new Regex(ValuePlaceholder);
        public static readonly Regex ReferencePlaceholderRegex = new Regex(@"\(\{(\w+)\}\)");

        public const string ValueGroupPrefix = "value";
        public const string ReferenceGroupPrefix = "reference";
        public const char GroupNamePartDelimiter = '_';
    }
}