using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    [TestFixture]
    public class StatReplacingParserTest
    {
        private IReadOnlyList<StatReplacerData> _statReplacers;

        [SetUp]
        public void SetUp()
        {
            _statReplacers = new[]
            {
                new StatReplacerData("Plain Stat", new[] { "stat1", "stat2", "stat3" }),
                new StatReplacerData("(grouped) (stat)", new[] { "$1", "$2", "$0" }),
                new StatReplacerData(".*[ck]omplex (.*) (regexp|regex)",
                    new[] { "complex", "$1", "$2" }),
                new StatReplacerData("removed", new string[0]),
            };
        }

        [Test]
        public void IsIParserIReadOnlyList()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            Assert.IsInstanceOf<IStringParser<IReadOnlyList<string>>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseWithoutReplacementPassesSuccessfullyParsed(bool innerSuccess)
        {
            var inner = StringParserTestUtils.MockParser("stat", innerSuccess, default, "").Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (actual, _, _) = sut.Parse("stat");

            return actual;
        }

        [Test]
        public void TryParseWithoutReplacementPassesRemaining()
        {
            const string expected = "remaining";
            var inner = StringParserTestUtils.MockParser("stat", default, expected, "").Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (_, actual, _) = sut.Parse("stat");

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryParseWithoutReplacementPassesResult()
        {
            const string expected = "result";
            var inner = StringParserTestUtils.MockParser("stat", default, default, expected).Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (_, _, actual) = sut.Parse("stat");

            Assert.That(actual, Has.Exactly(1).EqualTo(expected));
        }

        [TestCase(true, true, true, ExpectedResult = true)]
        [TestCase(true, true, false, ExpectedResult = false)]
        [TestCase(true, false, true, ExpectedResult = false)]
        [TestCase(false, true, true, ExpectedResult = false)]
        [TestCase(false, false, false, ExpectedResult = false)]
        public bool TryParseWithManyReplacementsReturnsCorrectSuccessfullyParsed(params bool[] successes)
        {
            var inner = StringParserTestUtils.MockParser(
                ("stat1", new StringParseResult<string>(successes[0], default, default)),
                ("stat2", new StringParseResult<string>(successes[1], default, default)),
                ("stat3", new StringParseResult<string>(successes[2], default, default))).Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (actual, _, _) = sut.Parse("plain stat");

            return actual;
        }

        [Test]
        public void TryParseWithManyReplacementsJoinsInnerRemainings()
        {
            string[] remainings = { "r1", "r2", "r3" };
            var inner = StringParserTestUtils.MockParser(
                ("stat1", new StringParseResult<string>(default, remainings[0], default)),
                ("stat2", new StringParseResult<string>(default, remainings[1], default)),
                ("stat3", new StringParseResult<string>(default, remainings[2], default))).Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (_, actual, _) = sut.Parse("plain stat");

            Assert.AreEqual(string.Join("\n", remainings), actual);
        }

        [Test]
        public void TryParseWithManyReplacementsReturnsListOfInnerResults()
        {
            string[] results = { "r1", "r2", "r3" };
            var inner = StringParserTestUtils.MockParser(
                ("stat1", new StringParseResult<string>(default, default, results[0])),
                ("stat2", new StringParseResult<string>(default, default, results[1])),
                ("stat3", new StringParseResult<string>(default, default, results[2]))).Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (_, _, actual) = sut.Parse("plain stat");

            CollectionAssert.AreEqual(results, actual);
        }

        [Test]
        public void TryParseWithEmptyReplacementReturnsSuccess()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            var (actual, _, _) = sut.Parse("removed");

            Assert.True(actual);
        }

        [Test]
        public void TryParseWithEmptyReplacementReturnsEmptyStringAsRemaining()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            var (_, actual, _) = sut.Parse("removed");

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void TryParseWithEmptyReplacementReturnsEmptyListAsResult()
        {
            var sut = new StatReplacingParser<string>(null, _statReplacers);

            var (_, _, actual) = sut.Parse("removed");

            CollectionAssert.IsEmpty(actual);
        }

        [TestCase("plain stat", new[] { "stat1", "stat2", "stat3" })]
        [TestCase("Plain Stat", new[] { "stat1", "stat2", "stat3" })]
        [TestCase("grouped stat", new[] { "grouped", "stat", "grouped stat" })]
        [TestCase("very complex asd regex", new[] { "complex", "asd", "regex" })]
        [TestCase("removed", new string[0])]
        [TestCase("unknown stat", new[] { "unknown stat" })]
        public void TryParseCallsInnerCorrectly(string stat, string[] expectedParts)
        {
            var setup = expectedParts.Select(s => (s, new StringParseResult<string>(default, default, default)));
            var innerMock = StringParserTestUtils.MockParser(setup.ToArray());

            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.Parse(stat);

            foreach (var expectedPart in expectedParts)
            {
                innerMock.VerifyParse(expectedPart);
            }
        }

        [Test]
        public void TryParseIgnoresWhitespaceRemainings()
        {
            string[] remainings = { "\t \n", "", "r3" };
            var inner = StringParserTestUtils.MockParser(
                ("stat1", new StringParseResult<string>(default, remainings[0], default)),
                ("stat2", new StringParseResult<string>(default, remainings[1], default)),
                ("stat3", new StringParseResult<string>(default, remainings[2], default))).Object;
            var sut = new StatReplacingParser<string>(inner, _statReplacers);

            var (_, actual, _) = sut.Parse("plain stat");

            Assert.AreEqual(remainings[2], actual);
        }

        [Test]
        public void TryParseMustFindFullMatchToReplaceStat()
        {
            var innerMock = StringParserTestUtils.MockParser("plain stat something", default, default, "");
            var sut = new StatReplacingParser<string>(innerMock.Object, _statReplacers);

            sut.Parse("plain stat something");

            innerMock.VerifyParse("plain stat something");
        }
    }
}