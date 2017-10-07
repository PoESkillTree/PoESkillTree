using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Tests.Builders.Matching
{
    [TestFixture]
    public class ResolvedMatchContextTest
    {
        [Test]
        public void IsIMatchContext()
        {
            var sut = Create();

            Assert.IsInstanceOf<IMatchContext<int>>(sut);
        }

        [Test]
        public void SingleThrowsWhenNoValues()
        {
            var sut = Create();

            object _;
            Assert.Throws<ParseException>(() => _ = sut.Single);
        }

        [Test]
        public void SingleThrowsWhenMultipleValues()
        {
            var sut = Create(1, 2, 3);

            object _;
            Assert.Throws<ParseException>(() => _ = sut.Single);
        }

        [TestCase(1, ExpectedResult = 1)]
        [TestCase(42, ExpectedResult = 42)]
        public int SingleReturnsCorrectResultWhenSingleValue(int value)
        {
            var sut = Create(value);

            return sut.Single;
        }

        [Test]
        public void FirstThrowsWhenNoValues()
        {
            var sut = Create();

            object _;
            Assert.Throws<ParseException>(() => _ = sut.First);
        }

        [TestCase(1, ExpectedResult = 1)]
        [TestCase(42, ExpectedResult = 42)]
        [TestCase(1, 2, 3, 4, ExpectedResult = 1)]
        public int FirstReturnsCorrectResultWhenValues(params int[] values)
        {
            var sut = Create(values);

            return sut.First;
        }

        [Test]
        public void LastThrowsWhenNoValues()
        {
            var sut = Create();

            object _;
            Assert.Throws<ParseException>(() => _ = sut.Last);
        }

        [TestCase(1, ExpectedResult = 1)]
        [TestCase(42, ExpectedResult = 42)]
        [TestCase(1, 2, 3, 4, ExpectedResult = 4)]
        public int LastReturnsCorrectResultWhenValues(params int[] values)
        {
            var sut = Create(values);

            return sut.Last;
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(1, 42)]
        public void IndexerThrowsWhenOutOfBounds(int index, params int[] values)
        {
            var sut = Create(values);

            object _;
            Assert.Throws<ParseException>(() => _ = sut[index]);
        }

        [TestCase(0, 42, ExpectedResult = 42)]
        [TestCase(0, 1, 2, 3, 4, ExpectedResult = 1)]
        [TestCase(1, 1, 2, 3, 4, ExpectedResult = 2)]
        [TestCase(2, 1, 2, 3, 4, ExpectedResult = 3)]
        [TestCase(3, 1, 2, 3, 4, ExpectedResult = 4)]
        public int IndexerReturnsCorrectResult(int index, params int[] values)
        {
            var sut = Create(values);

            return sut[index];
        }

        private static ResolvedMatchContext<int> Create(params int[] values)
        {
            return new ResolvedMatchContext<int>(values);
        }
    }
}