using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class CompositeParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = CreateSut(null);

            Assert.IsInstanceOf<IParser<string>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseReturnsSessionSuccessful(bool sessionSuccessful)
        {
            var innerParser = CreateConstant("");
            var session = Mock.Of<IParsingSession<int>>(s =>
                s.Successful == sessionSuccessful &&
                s.Completed &&
                s.CurrentParser == innerParser);
            var sut = CreateSut(session);

            return sut.TryParse("", out var _, out var _);
        }

        [Test]
        public void TryParseCallsSessionParseSuccessfulIfInnerCouldParse()
        {
            var sessionMock = new Mock<IParsingSession<int>>();
            sessionMock.SetupSequence(s => s.Completed)
                .Returns(false)
                .Returns(true);
            sessionMock.Setup(s => s.CurrentParser)
                .Returns(CreateConstant(""));
            var sut = CreateSut(sessionMock.Object);

            sut.TryParse("", out var _, out var _);

            sessionMock.Verify(s => s.ParseSuccessful());
        }

        [Test]
        public void TryParseCallsSessionParseFailedIfInnerCouldNotParse()
        {
            var sessionMock = new Mock<IParsingSession<int>>();
            sessionMock.SetupSequence(s => s.Completed)
                .Returns(false)
                .Returns(true);
            sessionMock.Setup(s => s.CurrentParser)
                .Returns(CreateConstant("", @return: false));
            var sut = CreateSut(sessionMock.Object);

            sut.TryParse("", out var _, out var _);

            sessionMock.Verify(s => s.ParseFailed());
        }

        [Test]
        public void TryParseOutputsCorrectValuesWithSingleInnerParse()
        {
            var sessionMock = new Mock<IParsingSession<int>>();
            sessionMock.SetupSequence(s => s.Completed)
                .Returns(false)
                .Returns(true);
            sessionMock.Setup(s => s.CurrentParser)
                .Returns(CreateConstant("stat", "remaining", 42));
            var sut = CreateSut(sessionMock.Object);

            sut.TryParse("stat", out var actualRemaining, out var actualResult);

            Assert.AreEqual("remaining", actualRemaining);
            Assert.AreEqual("42", actualResult);
        }

        [Test]
        public void TryParseOutputsCorrectValuesWithSingleFailedInnerParse()
        {
            var sessionMock = new Mock<IParsingSession<int>>();
            sessionMock.SetupSequence(s => s.Completed)
                .Returns(false)
                .Returns(true);
            sessionMock.Setup(s => s.CurrentParser)
                .Returns(CreateConstant("stat", "remaining", 42, false));
            var sut = CreateSut(sessionMock.Object);

            sut.TryParse("stat", out var actualRemaining, out var actualResult);

            Assert.AreEqual("remaining", actualRemaining);
            CollectionAssert.IsEmpty(actualResult);
        }

        [Test]
        public void TryParseOutputsCorrectValuesWithManyInnerParses()
        {
            var sessionMock = new Mock<IParsingSession<int>>();
            sessionMock.SetupSequence(s => s.Completed)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(true);
            var currentParser = 0;
            sessionMock.Setup(s => s.ParseSuccessful())
                .Callback(() => currentParser++);
            IParser<int>[] parsers =
            {
                CreateConstant("1 2 3", "2 3", 1),
                CreateConstant("2 3", "3", 2),
                CreateConstant("3", "", 3),
                CreateConstant("", "nothing", @return: false),
            };
            sessionMock.Setup(s => s.CurrentParser)
                .Returns(() => parsers[currentParser]);
            var sut = CreateSut(sessionMock.Object);

            sut.TryParse("1 2 3", out var actualRemaining, out var actualResult);

            Assert.AreEqual("nothing", actualRemaining);
            Assert.AreEqual("1,2,3", actualResult);
        }

        private static CompositeParser<int, string> CreateSut(IParsingSession<int> session)
        {
            string Aggregate(IEnumerable<int> results) => string.Join(",", results);

            return new CompositeParser<int, string>(
                new ConstantFactory<IParsingSession<int>>(session), Aggregate);
        }

        private static IParser<int> CreateConstant(string stat, 
            string remaining = "", int result = 0, bool @return = true)
        {
            var mock = new Mock<IParser<int>>();

            mock.Setup(p => p.TryParse(stat, out remaining, out result)).Returns(@return);

            return mock.Object;
        }
    }
}