using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Events
{
    [TestFixture]
    public class BufferingEventViewProviderTest
    {
        [Test]
        public void CreateReturnsCorrectInstance()
        {
            var defaultView = Mock.Of<ICountsSubsribers>();
            var bufferingView = Mock.Of<ICountsSubsribers>();

            var provider = BufferingEventViewProvider.Create(defaultView, bufferingView);

            Assert.AreSame(defaultView, provider.DefaultView);
            Assert.AreSame(bufferingView, provider.BufferingView);
        }

        [TestCase(42, 3)]
        [TestCase(0, 0)]
        public void CreateReturnsInstanceCalculatingSubscriberCountCorrectly(int defaultCount, int suspendableCount)
        {
            var defaultView = Mock.Of<ICountsSubsribers>(o => o.SubscriberCount == defaultCount);
            var bufferingView = Mock.Of<ICountsSubsribers>(o => o.SubscriberCount == suspendableCount);
            var provider = BufferingEventViewProvider.Create(defaultView, bufferingView);

            var actual = provider.SubscriberCount;

            Assert.AreEqual(defaultCount + suspendableCount, actual);
        }
    }
}