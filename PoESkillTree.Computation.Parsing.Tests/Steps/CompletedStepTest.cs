using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class CompletedStepTest
    {
        [Test]
        public void IsISession()
        {
            var sut = new CompletedStep<int, bool>(false, 0);

            Assert.IsInstanceOf<IStep<int, bool>>(sut);
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = true)]
        public bool CompletedAlwaysReturnsTrue(bool successful)
        {
            var sut = new CompletedStep<int, bool>(successful, 0);

            return sut.Completed;
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool SuccessfulReturnConstructorParameter(bool successful)
        {
            var sut = new CompletedStep<int, bool>(successful, 0);

            return sut.Successful;
        }

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(42, ExpectedResult = 42)]
        public int CurrentStepReturnsConstructorParameter(int step)
        {
            var sut = new CompletedStep<int, bool>(true, step);

            return sut.Current;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NextAlwaysReturnsSelf(bool data)
        {
            var sut = new CompletedStep<int, bool>(true, 0);

            Assert.AreSame(sut, sut.Next(data));
        }

        [TestCase(true, 0)]
        [TestCase(true, 42)]
        [TestCase(false, 42)]
        public void SutIsEqualIfEqualConstructorParameters(bool successful, int step)
        {
            var sut = new CompletedStep<int, bool>(successful, step);
            var other = new CompletedStep<int, bool>(successful, step);

            Assert.True(sut.Equals(other));
        }

        [TestCase(true, 0, false, 0)]
        [TestCase(true, 0, true, 42)]
        [TestCase(true, 0, false, 42)]
        public void SutIsNotEqualIfNotEqualConstructorParameters(bool sutSuccessful, int sutStep,
            bool otherSuccessful, int otherStep)
        {
            var sut = new CompletedStep<int, bool>(sutSuccessful, sutStep);
            var other = new CompletedStep<int, bool>(otherSuccessful, otherStep);

            Assert.False(sut.Equals(other));
        }

        [Test]
        public void SutIsNotEqualToOtherObject()
        {
            var sut = new CompletedStep<int, bool>(true, 42);

            Assert.False(sut.Equals(new object()));
        }
    }
}