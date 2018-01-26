using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class SuspendableEventViewProviderTest
    {
        [Test]
        public void CreateReturnsCorrectInstance()
        {
            var defaultView = Mock.Of<ICalculationNode>();
            var suspendableView = Mock.Of<ICachingNode>();

            var provider = SuspendableEventViewProvider.Create(defaultView, suspendableView);

            Assert.AreSame(defaultView, provider.DefaultView);
            Assert.AreSame(suspendableView, provider.SuspendableView);
            Assert.AreSame(suspendableView, provider.Suspender);
        }
    }
}