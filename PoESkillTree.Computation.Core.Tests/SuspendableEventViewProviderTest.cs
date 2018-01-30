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
            var defaultView = Mock.Of<ICountsSubsribers>();
            var suspendableView = Mock.Of<ISuspendableCountsSubscribers>();

            var provider = SuspendableEventViewProvider.Create(defaultView, suspendableView);

            Assert.AreSame(defaultView, provider.DefaultView);
            Assert.AreSame(suspendableView, provider.SuspendableView);
            Assert.AreSame(suspendableView, provider.Suspender);
        }

        [TestCase(42, 3)]
        [TestCase(0, 0)]
        public void CreateReturnsInstanceCalculatingSubscriberCountCorrectly(int defaultCount, int suspendableCount)
        {
            var defaultView = Mock.Of<ICountsSubsribers>(o => o.SubscriberCount == defaultCount);
            var suspendableView = Mock.Of<ISuspendableCountsSubscribers>(o => o.SubscriberCount == suspendableCount);
            var provider = SuspendableEventViewProvider.Create(defaultView, suspendableView);

            var actual = provider.SubscriberCount;

            Assert.AreEqual(defaultCount + suspendableCount, actual);
        }
    }

    public interface ISuspendableCountsSubscribers : ISuspendableEvents, ICountsSubsribers
    {
    }
}