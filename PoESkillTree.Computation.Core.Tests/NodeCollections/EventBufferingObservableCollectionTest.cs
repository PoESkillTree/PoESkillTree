using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    [TestFixture]
    public class EventBufferingObservableCollectionTest
    {
        [Test]
        public void SutIsObservableCollection()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ObservableCollection<int>>(sut);
        }

        [Test]
        public void CollectionChangedIsBuffered()
        {
            var sut = CreateSut();
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(new[] { -1 }, args.AddedItems);
                Assert.IsEmpty(args.RemovedItems);
                raised = true;
            };

            sut.Add(-1);

            Assert.IsTrue(raised);
        }

        [Test]
        public void CollectionChangedIsCombinedWhenBufferingMultipleAdds()
        {
            var eventBuffer = new EventBuffer();
            eventBuffer.StartBuffering();
            var sut = CreateSut(eventBuffer);
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(new[] { -1, -2 }, args.AddedItems);
                Assert.IsEmpty(args.RemovedItems);
                raised = true;
            };

            sut.Add(-1);
            sut.Add(-2);
            eventBuffer.Flush();

            Assert.IsTrue(raised);
        }

        [Test]
        public void CollectionChangedIsCombinedWhenBufferingMultipleAddsAndRemoves()
        {
            var eventBuffer = new EventBuffer();
            var sut = CreateSut(eventBuffer);
            sut.Add(3);
            eventBuffer.StartBuffering();
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(new[] { -1, -2 }, args.AddedItems);
                Assert.AreEqual(new[] { 3 }, args.RemovedItems);
                raised = true;
            };

            sut.Add(-1);
            sut.Add(-1);
            sut.Add(-2);
            sut.Remove(-2);
            sut.Remove(3);
            sut.Add(-2);
            sut.Add(3);
            sut.Remove(3);
            eventBuffer.Flush();

            Assert.IsTrue(raised);
        }

        private static EventBufferingObservableCollection<int> CreateSut(IEventBuffer eventBuffer = null)
            => new EventBufferingObservableCollection<int>(eventBuffer ?? new EventBuffer());
    }
}