using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class CompositeParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = CreateSut(null);

            Assert.IsInstanceOf<IParser<IReadOnlyList<int>>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseReturnsStepSuccessfulWithCompletedInitialStep(bool stepSuccessful)
        {
            var initialStep = SetupSequenceWithSuccessful(stepSuccessful);
            var sut = CreateSut(initialStep);

            return sut.TryParse("", out var _, out var _);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseReturnsLastStepsSuccessful(bool lastStepSuccessful)
        {
            var initialStep = SetupSequenceWithSuccessful(lastStepSuccessful, true, false);
            var sut = CreateSut(initialStep);

            return sut.TryParse("stat", out var _, out var _);
        }

        [Test]
        public void TryParseOutputsCorrectValuesWithSingleInnerParse()
        {
            var initialStep = SetupSequence(true);
            var sut = CreateSut(initialStep);

            sut.TryParse("stat", out var actualRemaining, out var actualResult);

            Assert.AreEqual("remaining", actualRemaining);
            Assert.That(actualResult, Has.Exactly(1).EqualTo(42));
        }

        [Test]
        public void TryParseOutputsCorrectValuesWithSingleFailedInnerParse()
        {
            var initialStep = SetupSequence(false);
            var sut = CreateSut(initialStep);

            sut.TryParse("stat", out var actualRemaining, out var actualResult);

            Assert.AreEqual("remaining", actualRemaining);
            CollectionAssert.IsEmpty(actualResult);
        }

        [Test]
        public void TryParseOutputsCorrectValuesWithManyInnerParses()
        {
            IParser<int>[] parsers =
            {
                MockConstantParser("1 2 3", "2 3", 1),
                MockConstantParser("2 3", "3", 2),
                MockConstantParser("3", "", 3),
                MockConstantParser("", "nothing", @return: false),
            };
            var steps = new IStep<IParser<int>, bool>[5];
            steps[4] = CreateCompletedStep();
            for (var i = steps.Length - 2; i >= 0; i--)
            {
                var parserReturn = i < 3;
                steps[i] = MockStep(parsers[i], parserReturn, steps[i + 1]);
            }
            var sut = CreateSut(steps[0]);

            sut.TryParse("1 2 3", out var actualRemaining, out var actualResult);

            Assert.AreEqual("nothing", actualRemaining);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, actualResult);
        }

        private static CompositeParser<int> CreateSut(IStep<IParser<int>, bool> initialStep)
        {
            return new CompositeParser<int>(initialStep);
        }

        private static IParser<int> MockConstantParser(string stat, 
            string remaining = "", int result = 0, bool @return = true)
        {
            var mock = new Mock<IParser<int>>();

            mock.Setup(p => p.TryParse(stat, out remaining, out result)).Returns(@return);

            return mock.Object;
        }

        private static IStep<IParser<int>, bool> MockStep(IParser<int> parser, bool parserReturn,
            IStep<IParser<int>, bool> nextStep)
        {
            return Mock.Of<IStep<IParser<int>, bool>>(s =>
                !s.Completed &&
                s.Current == parser &&
                s.Next(parserReturn) == nextStep);
        }

        private static IStep<IParser<int>, bool> CreateCompletedStep(bool successful = false)
        {
            return new CompletedStep<IParser<int>, bool>(successful, new NoOpParser<int>());
        }

        private static IStep<IParser<int>, bool> SetupSequence(params bool[] parserReturns)
        {
            return SetupSequenceWithSuccessful(false, parserReturns);
        }

        private static IStep<IParser<int>, bool> SetupSequenceWithSuccessful(
            bool lastStepSuccessful, params bool[] parserReturns)
        {
            return SetupSequenceWithSuccessful(lastStepSuccessful,
                (IReadOnlyCollection<bool>) parserReturns);
        }

        private static IStep<IParser<int>, bool> SetupSequenceWithSuccessful(
            bool lastStepSuccessful, IReadOnlyCollection<bool> parserReturns)
        {
            if (parserReturns.IsEmpty())
            {
                return CreateCompletedStep(lastStepSuccessful);
            }
            var nextStep = SetupSequenceWithSuccessful(lastStepSuccessful, 
                parserReturns.Skip(1).ToList());
            var @return = parserReturns.First();
            var parser = MockConstantParser("stat", "remaining", 42, @return);
            return MockStep(parser, @return, nextStep);
        }


        private class NoOpParser<TResult> : IParser<TResult>
        {
            public bool TryParse(string stat, out string remaining, out TResult result)
            {
                remaining = stat;
                result = default;
                return false;
            }
        }
    }
}