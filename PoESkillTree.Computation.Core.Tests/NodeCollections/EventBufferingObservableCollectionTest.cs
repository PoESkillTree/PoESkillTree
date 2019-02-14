using System.ComponentModel;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;

namespace PoESkillTree.Computation.Core.Tests.NodeCollections
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
            var eventBufferMock = new Mock<IEventBuffer>();
            var sut = CreateSut(eventBufferMock.Object);

            sut.Add(-1);

            eventBufferMock.Verify(b => b.Buffer(sut,
                It.Is<CollectionChangeEventArgs>(e => e.Action == CollectionChangeAction.Add)));
        }

        [Test]
        public void CollectionChangedIsRefreshWhenBufferingMultiple()
        {
            var eventBuffer = new EventBuffer();
            eventBuffer.StartBuffering();
            var sut = CreateSut(eventBuffer);
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(CollectionChangeAction.Refresh, args.Action);
                Assert.IsNull(args.Element);
            };

            sut.Add(-1);
            sut.Add(-2);
            
            eventBuffer.Flush();
        }

        private static EventBufferingObservableCollection<int> CreateSut(IEventBuffer eventBuffer = null)
            => new EventBufferingObservableCollection<int>(eventBuffer ?? new EventBuffer());
    }
}