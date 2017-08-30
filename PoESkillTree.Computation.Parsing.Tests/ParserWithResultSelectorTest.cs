using System;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ParserWithResultSelectorTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = Create(null);

            Assert.IsInstanceOf<IParser<string>>(sut);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TryParsePassesReturn(bool expected)
        {
            var innerMock = new Mock<IParser<int>>();
            string _;
            int innerResult;
            innerMock.Setup(p => p.TryParse("stat", out _, out innerResult)).Returns(expected);
            var sut = Create(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("remaining")]
        [TestCase("re mai ning")]
        public void TryParsesPassesRemaining(string expected)
        {
            var innerMock = new Mock<IParser<int>>();
            int _;
            innerMock.Setup(p => p.TryParse("stat", out expected, out _));
            var sut = Create(innerMock.Object);

            sut.TryParse("stat", out var actual, out var _);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(42, 2)]
        [TestCase(42, 10)]
        [TestCase(5, 3)]
        public void TryParseAppliesSelectorToResult(int innerResult, int summand)
        {
            string Select(int i) => (i + summand).ToString();

            var expected = Select(innerResult);
            var innerMock = new Mock<IParser<int>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out _, out innerResult));
            var sut = Create(innerMock.Object, Select);

            sut.TryParse("stat", out var _, out var actual);

            Assert.AreEqual(expected, actual);
        }

        private static ParserWithResultSelector<int, string> Create(IParser<int> inner)
        {
            return Create(inner, i => i.ToString());
        }

        private static ParserWithResultSelector<int, string> Create(IParser<int> inner, 
            Func<int, string> selector)
        {
            return new ParserWithResultSelector<int, string>(inner, selector);
        }
    }
}