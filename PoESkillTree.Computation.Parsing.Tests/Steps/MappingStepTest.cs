using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class MappingStepTest
    {
        [Test]
        public void IsIStep()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IStep<string, bool>>(sut);
        }

        [Test]
        public void InnerReturnsCorrectResult()
        {
            var inner = new CountingStep(2, 3);
            var sut = CreateSut(inner);

            Assert.AreEqual(inner, sut.Inner);
        }

        [TestCase(2, 3, ExpectedResult = false)]
        [TestCase(3, 3, ExpectedResult = true)]
        public bool CompletedReturnsInnerCompleted(int current, int maximum)
        {
            var sut = CreateSut(current, maximum);

            return sut.Completed;
        }

        [TestCase(2, 3, true, ExpectedResult = true)]
        [TestCase(3, 3, true, ExpectedResult = true)]
        [TestCase(2, 3, false, ExpectedResult = false)]
        public bool CompletedReturnsInnerSuccessful(int current, int maximum, bool data)
        {
            var inner = new CountingStep(current, maximum).Next(data);
            var sut = CreateSut(inner);

            return sut.Successful;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NextInnerReturnsInnerNext(bool data)
        {
            var inner = new CountingStep(0, 3);
            var sut = CreateSut(inner);

            var next = (MappingStep<int, string, bool>) sut.Next(data);

            Assert.AreEqual(inner.Next(data), next.Inner);
        }

        [TestCase(2, 3, ExpectedResult = "2")]
        [TestCase(3, 3, ExpectedResult = "3")]
        public string CurrentReturnsCorrectResult(int current, int maximum)
        {
            var sut = CreateSut(current, maximum);

            return sut.Current;
        }

        private static MappingStep<int, string, bool> CreateSut(int current = 2, int maximum = 3)
        {
            var inner = new CountingStep(current, maximum);
            return CreateSut(inner);
        }

        private static MappingStep<int, string, bool> CreateSut(IStep<int, bool> inner)
        {
            return new MappingStep<int, string, bool>(inner, i => i.ToString());
        }


        private class CountingStep : IStep<int, bool>
        {
            private readonly int _maximum;

            public CountingStep(int current, int maximum)
            {
                _maximum = maximum;
                Current = current;
            }

            public bool Completed => Current >= _maximum;

            public bool Successful { get; } = true;

            public int Current { get; }

            public IStep<int, bool> Next(bool data)
            {
                if (!data)
                {
                    return new CompletedStep<int, bool>(false, Current);
                }
                return new CountingStep(Current + 1, _maximum);
            }

            private bool Equals(CountingStep other)
            {
                return _maximum == other._maximum && Current == other.Current;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((CountingStep) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_maximum * 397) ^ Current;
                }
            }
        }
    }
}