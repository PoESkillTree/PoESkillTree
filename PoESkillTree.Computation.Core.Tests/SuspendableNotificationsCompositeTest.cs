using System.Collections.Generic;
using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class SuspendableNotificationsCompositeTest
    {
        [Test]
        public void SutIsSuspendableNotifications()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ISuspendableNotifications>(sut);
        }

        [Test]
        public void SuspendNotificationsCallsAllChildren()
        {
            var children = MockManyChildren();
            var sut = CreateSut();
            children.Select(m => m.Object).ForEach(sut.Add);

            sut.SuspendNotifications();

            children.ForEach(m => m.Verify(c => c.SuspendNotifications()));
        }

        [Test]
        public void ResumeNotificationsCallsAllChildren()
        {
            var children = MockManyChildren();
            var sut = CreateSut();
            children.Select(m => m.Object).ForEach(sut.Add);

            sut.ResumeNotifications();

            children.ForEach(m => m.Verify(c => c.ResumeNotifications()));
        }

        [Test]
        public void RemoveRemovesChildren()
        {
            var children = MockManyChildren();
            var sut = CreateSut();
            children.Select(m => m.Object).ForEach(sut.Add);

            sut.Remove(children[1].Object);

            sut.SuspendNotifications();
            children[1].Verify(c => c.SuspendNotifications(), Times.Never);
        }

        private SuspendableNotificationsComposite CreateSut()
            => new SuspendableNotificationsComposite();

        private static IReadOnlyList<Mock<ISuspendableNotifications>> MockManyChildren() => 
            Enumerable.Range(0, 3).Select(_ => new Mock<ISuspendableNotifications>()).ToList();
    }
}