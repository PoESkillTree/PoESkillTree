using System;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Thrown if there are errors withing the data specification, e.g. referencing values that don't occur in the
    /// matched stat.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}