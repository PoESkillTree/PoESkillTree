using System;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    public abstract class ParsingStepTest
    {
        protected abstract IStep<ParsingStep, bool> Sut { get; }
        protected abstract IStep<ParsingStep, bool> ExpectedNextFalse { get; }
        protected abstract IStep<ParsingStep, bool> ExpectedNextTrue { get; }

        [Test]
        public void IsIStep()
        {
            Assert.IsInstanceOf<IStep<ParsingStep, bool>>(Sut);
        }

        [Test(ExpectedResult = false)]
        public bool CompletedReturnsFalse()
        {
            return Sut.Completed;
        }

        [Test(ExpectedResult = false)]
        public bool SuccessfulReturnsFalse()
        {
            return Sut.Successful;
        }

        [Test]
        public void CurrentReturnsCorrectResult()
        {
            var expected = Enum.GetValues(typeof(ParsingStep)).Cast<ParsingStep>()
                .Where(s => Sut.GetType().Name.StartsWith(s.ToString()))
                .MaxBy(s => s.ToString().Length);

            var actual = Sut.Current;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextFalseReturnsCorrectResult()
        {
            var actual = Sut.Next(false);

            Assert.AreEqual(ExpectedNextFalse, actual);
        }

        [Test]
        public void NextTrueReturnsCorrectResult()
        {
            var actual = Sut.Next(true);

            Assert.AreEqual(ExpectedNextTrue, actual);
        }
    }
}