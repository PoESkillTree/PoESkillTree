using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.StringParsers;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Data object for the return value of <see cref="IStringParser{TResult}.Parse"/>. Supports deconstruction to tuples and
    /// implicit conversion from tuples, allowing it to generally behave like a tuple, just with a proper type name.
    /// </summary>
    /// <typeparam name="T">The type of parsing results.</typeparam>
    public class ParseResult<T>
    {
        /// <summary>
        /// True if the stat was parsed successfully.
        /// </summary>
        public bool SuccessfullyParsed { get; }

        /// <summary>
        /// The parts of the stat not parsed into <see cref="Result"/>.
        /// </summary>
        public string RemainingStat { get; }

        /// <summary>
        /// The parsing result. Only defined if <see cref="SuccessfullyParsed"/> is true.
        /// </summary>
        public T Result { get; }

        public ParseResult(bool successfullyParsed, string remainingStat, T result)
        {
            SuccessfullyParsed = successfullyParsed;
            RemainingStat = remainingStat;
            Result = result;
        }

        public void Deconstruct(out bool successfullyParsed, out string remainingStat, out T result)
        {
            successfullyParsed = SuccessfullyParsed;
            remainingStat = RemainingStat;
            result = Result;
        }

        public static implicit operator ParseResult<T>((bool successfullyParsed, string remainingStat, T result) t)
        {
            return new ParseResult<T>(t.successfullyParsed, t.remainingStat, t.result);
        }
    }

    /// <summary>
    /// Data object for the return value of <see cref="IParser.Parse"/>. Supports deconstruction to tuples.
    /// </summary>
    public class ParseResult : ParseResult<IReadOnlyList<Modifier>>
    {
        public ParseResult(bool successfullyParsed, string remainingStat, IReadOnlyList<Modifier> result)
            : base(successfullyParsed, remainingStat, result)
        {
        }
    }
}