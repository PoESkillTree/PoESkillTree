using System;
using System.Collections.Generic;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Parsing.StringParsers;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.Tests.StringParsers
{
    [TestFixture]
    public class CompositeParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = CreateSut(null, null);

            Assert.IsInstanceOf<IStringParser<IReadOnlyList<int>>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseReturnsStepSuccessfulWithCompletedInitialStep(bool stepSuccessful)
        {
            var stepper = MockStepper("s", "s", stepSuccessful);
            var sut = CreateSut(stepper, null);

            var (actual, _, _) = sut.Parse("");

            return actual;
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool TryParseReturnsLastStepsSuccessful(bool lastStepSuccessful)
        {
            var sut = SetupSequenceWithSuccessful(lastStepSuccessful, true, false);

            var (actual, _, _) = sut.Parse("stat0");

            return actual;
        }

        [Test]
        public void TryParseReturnsCorrectRemainingAndStatWithSingleInnerParse()
        {
            var sut = SetupSequence(true);

            var (_, actualRemaining, actualResult) = sut.Parse("stat0");

            Assert.AreEqual("stat1", actualRemaining);
            Assert.That(actualResult, Has.Exactly(1).EqualTo(42));
        }

        [Test]
        public void TryParseReturnsCorrectRemainingAndStatWithSingleFailedInnerParse()
        {
            var sut = SetupSequence(false);

            var (_, actualRemaining, actualResult) = sut.Parse("stat0");

            Assert.AreEqual("stat1", actualRemaining);
            CollectionAssert.IsEmpty(actualResult);
        }

        [Test]
        public void TryParseReturnsCorrectRemainingAndStatWithManyInnerParses()
        {
            IStringParser<int>[] parsers =
            {
                MockConstantParser("1 2 3", "2 3", 1),
                MockConstantParser("2 3", "3", 2),
                MockConstantParser("3", "", 3),
                MockConstantParser("", "nothing", @return: false),
            };
            var successTransitions = new Dictionary<string, string>();
            var failureTransitions = new Dictionary<string, string>();
            for (int i = 0; i < 4; i++)
            {
                var step = i.ToString();
                var nextStep = (i + 1).ToString();
                if (i < 3)
                {
                    successTransitions[step] = nextStep;
                }
                else
                {
                    failureTransitions[step] = nextStep;
                }
            }

            var stepper = MockStepper("0", "4", false, successTransitions, failureTransitions);
            IStringParser<int> StepToParser(string step) => parsers[int.Parse(step)];

            var sut = CreateSut(stepper, StepToParser);

            var (_, actualRemaining, actualResult) = sut.Parse("1 2 3");

            Assert.AreEqual("nothing", actualRemaining);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, actualResult);
        }

        private static CompositeParser<int, string> CreateSut(
            IStepper<string> stepper, Func<string, IStringParser<int>> stepToParserFunc)
        {
            return new CompositeParser<int, string>(stepper, stepToParserFunc);
        }

        private static IStringParser<int> MockConstantParser(string stat, 
            string remaining = "remaining", int result = 42, bool @return = true)
        {
            return StringParserTestUtils.MockParser(stat, @return, remaining, result).Object;
        }

        private static IStepper<string> MockStepper(
            string initial, string terminal, bool terminalIsSuccess,
            IReadOnlyDictionary<string, string> successTransitions = null,
            IReadOnlyDictionary<string, string> failureTransitions = null)
        {
            var mock = new Mock<IStepper<string>>();

            mock.SetupGet(s => s.InitialStep).Returns(initial);
            mock.Setup(s => s.IsSuccess(terminal)).Returns(terminalIsSuccess);
            mock.Setup(s => s.IsTerminal(terminal)).Returns(true);

            if (successTransitions != null)
            {
                foreach (var (k, v) in successTransitions)
                {
                    mock.Setup(s => s.NextOnSuccess(k)).Returns(v);
                }
            }

            if (failureTransitions != null)
            {
                foreach (var (k, v) in failureTransitions)
                {
                    mock.Setup(s => s.NextOnFailure(k)).Returns(v);
                }
            }

            return mock.Object;
        }

        private static  CompositeParser<int, string> SetupSequence(params bool[] parserReturns)
        {
            return SetupSequenceWithSuccessful(false, parserReturns);
        }

        private static  CompositeParser<int, string> SetupSequenceWithSuccessful(
            bool lastStepSuccessful, params bool[] parserReturns)
        {
            return SetupSequenceWithSuccessful(lastStepSuccessful, (IReadOnlyCollection<bool>) parserReturns);
        }

        private static CompositeParser<int, string> SetupSequenceWithSuccessful(
            bool lastStepSuccessful, IReadOnlyCollection<bool> parserReturns)
        {
            var successTransitions = new Dictionary<string, string>();
            var failureTransitions = new Dictionary<string, string>();
            var parsers = new Dictionary<string, IStringParser<int>>();
            foreach (var (i, parserReturn) in parserReturns.Index())
            {
                var step = i.ToString();
                var nextStep = (i + 1).ToString();
                if (parserReturn)
                {
                    successTransitions[step] = nextStep;
                }
                else
                {
                    failureTransitions[step] = nextStep;
                }

                parsers[step] = MockConstantParser("stat" + step, "stat" + nextStep, @return: parserReturn);
            }

            var stepper = MockStepper("0", parserReturns.Count.ToString(), lastStepSuccessful, successTransitions,
                failureTransitions);

            return CreateSut(stepper, k => parsers[k]);
        }
    }
}