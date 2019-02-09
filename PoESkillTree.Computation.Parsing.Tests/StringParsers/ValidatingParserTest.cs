using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.StringParsers;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.Tests.StringParsers
{
    [TestFixture]
    public class ValidatingParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = new ValidatingParser<string>(null);

            Assert.IsInstanceOf<IStringParser<string>>(sut);
        }

        [TestCase(true, "remaining", "result")]
        [TestCase(false, "remaining", "result")]
        [TestCase(true, " ", "result")]
        [TestCase(false, " ", "result")]
        public void TryParsePassesResult(bool innerSuccess, string innerRemaining, string result)
        {
            var inner = StringParserTestUtils.MockParser("stat", innerSuccess, innerRemaining, result).Object;
            var sut = new ValidatingParser<string>(inner);

            var (_, _, actual) = sut.Parse("stat");

            Assert.AreEqual(result, actual);
        }

        [Test]
        public void TryParseReturnsFailureIfInnerReturnsFailure()
        {
            const string remaining = " ";
            var inner = StringParserTestUtils.MockParser("stat", false, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.False(actual);
        }

        [Test]
        public void TryParseReturnsFailureIfRemainingContainsNonWhiteSpace()
        {
            const string remaining = " r ";
            var inner = StringParserTestUtils.MockParser("stat", true, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.False(actual);
        }

        [Test]
        public void TryParseReturnsFailureIfRemainingStartsWithHidden()
        {
            const string remaining = ItemConstants.HiddenStatSuffix + " ";
            var inner = StringParserTestUtils.MockParser("stat", true, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.False(actual);
        }

        [Test]
        public void TryParseReturnsSuccessIfRemainingIsOnlyWhiteSpace()
        {
            const string remaining = " \n\t\r";
            var inner = StringParserTestUtils.MockParser("stat", true, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.True(actual);
        }

        [Test]
        public void TryParseReturnsSuccessIfRemainingIsOnlyWhiteSpaceAndEndsWithHidden()
        {
            const string remaining = " \n\t\r" + ItemConstants.HiddenStatSuffix;
            var inner = StringParserTestUtils.MockParser("stat", true, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.True(actual);
        }

        [Test]
        public void TryParseHiddenTestIsCaseInsensitive()
        {
            var remaining = ItemConstants.HiddenStatSuffix.ToLowerInvariant();
            var inner = StringParserTestUtils.MockParser("stat", true, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (actual, _, _) = sut.Parse("stat");

            Assert.True(actual);
        }

        [TestCase("a b c")]
        [TestCase(ItemConstants.HiddenStatSuffix + " a")]
        public void TryParseReturnsCorrectUnchangedRemaining(string remaining)
        {
            var inner = StringParserTestUtils.MockParser("stat", true, remaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (_, actual, _) = sut.Parse("stat");

            Assert.AreEqual(remaining, actual);
        }

        [TestCase(" r ", ExpectedResult = "r")]
        [TestCase(ItemConstants.HiddenStatSuffix + " ", ExpectedResult = ItemConstants.HiddenStatSuffix)]
        [TestCase(" \n\t\r", ExpectedResult = "")]
        [TestCase(" \n\t\r " + ItemConstants.HiddenStatSuffix, ExpectedResult = "")]
        [TestCase(" \ntest\t\r " + ItemConstants.HiddenStatSuffix, ExpectedResult = "test")]
        public string TryParseReturnsCorrectRemaining(string innerRemaining)
        {
            var inner = StringParserTestUtils.MockParser("stat", true, innerRemaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (_, actual, _) = sut.Parse("stat");

            return actual;
        }

        [Test]
        public void TryParseCleansIfInnerReturnsFalse()
        {
            const string innerRemaining = " \ntest\t\r " + ItemConstants.HiddenStatSuffix;
            var inner = StringParserTestUtils.MockParser("stat", true, innerRemaining, "").Object;
            var sut = new ValidatingParser<string>(inner);

            var (_, actual, _) = sut.Parse("stat");

            Assert.AreEqual("test", actual);
        }
    }
}