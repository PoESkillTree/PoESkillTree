using System;

namespace PoESkillTree.Computation.Common.Parsing
{
    /// <summary>
    /// Thrown if there are errors withing the data specification, e.g. referencing values that don't occur in the
    /// matched stat or using builders in a way that is not (yet) supported.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}