using System.Collections.Generic;
using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.Tests.Events
{
    [TestFixture]
    public class SuspendableEventsCompositeTest
    {
        [Test]
        public void SutIsSuspendableEvents()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ISuspendableEvents>(sut);
        }

        [Test]
        public void SuspendEventsCallsAllChildren()
        {
            var children = MockManyChildren();
            var sut = CreateSut();
            children.Select(m => m.Object).ForEach(sut.Add);

            sut.SuspendEvents();

            children.ForEach(m => m.Verify(c => c.SuspendEvents()));
        }

        [Test]
        public void ResumeEventsCallsAllChildren()
        {
            var children = MockManyChildren();
            var sut = CreateSut();
            children.Select(m => m.Object).ForEach(sut.Add);

            sut.ResumeEvents();

            children.ForEach(m => m.Verify(c => c.ResumeEvents()));
        }

        [Test]
        public void RemoveRemovesChildren()
        {
            var children = MockManyChildren();
            var sut = CreateSut();
            children.Select(m => m.Object).ForEach(sut.Add);

            sut.Remove(children[1].Object);

            sut.SuspendEvents();
            children[1].Verify(c => c.SuspendEvents(), Times.Never);
        }

        private SuspendableEventsComposite CreateSut()
            => new SuspendableEventsComposite();

        private static IReadOnlyList<Mock<ISuspendableEvents>> MockManyChildren() => 
            Enumerable.Range(0, 3).Select(_ => new Mock<ISuspendableEvents>()).ToList();
    }
}