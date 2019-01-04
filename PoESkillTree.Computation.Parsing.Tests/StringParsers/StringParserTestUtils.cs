using System;
using Moq;
using PoESkillTree.Computation.Parsing.StringParsers;

namespace PoESkillTree.Computation.Parsing.Tests.StringParsers
{
    public static class StringParserTestUtils
    {
        public static StringParseResult<TResult> Parse<TResult>(this IStringParser<TResult> @this,
            string modifierLine)
            => @this.Parse(modifierLine, default, default);

        public static Mock<IStringParser<TResult>> MockParser<TResult>(
            string modifier, bool successfullyParsed, string remainingSubstring, TResult result)
            => MockParser(modifier, new StringParseResult<TResult>(successfullyParsed, remainingSubstring, result));

        private static Mock<IStringParser<TResult>> MockParser<TResult>(
            string modifier, StringParseResult<TResult> result)
            => MockParser((modifier, result));

        public static Mock<IStringParser<TResult>> MockParser<TResult>(
            params (string modifier, StringParseResult<TResult> result)[] setup)
        {
            var mock = new Mock<IStringParser<TResult>>();
            foreach (var (modifier, result) in setup)
            {
                mock.Setup(p => p.Parse(CreateParameter(modifier)))
                    .Returns(result);
            }
            return mock;
        }

        public static void VerifyParse<TResult>(this Mock<IStringParser<TResult>> @this,
            string modifier, Func<Times> times)
            => @this.Verify(p => p.Parse(CreateParameter(modifier)), times);

        public static void VerifyParse<TResult>(this Mock<IStringParser<TResult>> @this, string modifier)
            => @this.Verify(p => p.Parse(CreateParameter(modifier)));

        private static CoreParserParameter CreateParameter(string modifier)
            => new CoreParserParameter(modifier, default, default);
    }
}