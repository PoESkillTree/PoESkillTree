using System;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.StringParsers;

namespace PoESkillTree.Computation.Parsing.Tests.StringParsers
{
    [TestFixture]
    public class ResultMappingParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = Create(null);

            Assert.IsInstanceOf<IStringParser<string>>(sut);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TryParsePassesSuccessfullyParsed(bool expected)
        {
            var inner = StringParserTestUtils.MockParser("stat", expected, default, 0).Object;
            var sut = Create(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        [TestCase("remaining")]
        [TestCase("re mai ning")]
        public void TryParsesPassesRemaining(string expected)
        {
            var inner = StringParserTestUtils.MockParser("stat", default, expected, 0).Object;
            var sut = Create(inner);

            var (_, actual, _) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        [TestCase(42, 2)]
        [TestCase(42, 10)]
        [TestCase(5, 3)]
        public void TryParseAppliesSelectorToResult(int innerResult, int summand)
        {
            string Select(int i) => (i + summand).ToString();

            var expected = Select(innerResult);
            var inner = StringParserTestUtils.MockParser("stat", default, default, innerResult).Object;
            var sut = Create(inner, Select);

            var (_, _, actual) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        private static ResultMappingParser<int, string> Create(IStringParser<int> inner)
        {
            return Create(inner, i => i.ToString());
        }

        private static ResultMappingParser<int, string> Create(IStringParser<int> inner, 
            Func<int, string> selector)
        {
            return new ResultMappingParser<int, string>(inner, (_, i) => selector(i));
        }
    }
}