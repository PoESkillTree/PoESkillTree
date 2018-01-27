using NUnit.Framework;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class SuspendableNodeCollectionTest
    {
        [Test]
        public void SutIsNodeCollection()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<NodeCollection<int>>(sut);
        }

        [Test]
        public void SutIsISuspendableEvents()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ISuspendableEvents>(sut);
        }

        [Test]
        public void CollectionChangedIsSuppressedAfterSuspendEvents()
        {
            var sut = CreateSut();

            sut.SuspendEvents();

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            RaiseCollectionChanged(sut);
        }

        [Test]
        public void CollectionChangedIsNotSuppressedByDefault()
        {
            var sut = CreateSut();
            var raised = false;
            sut.CollectionChanged += (sender, args) => raised = true;

            RaiseCollectionChanged(sut);
            Assert.IsTrue(raised);
        }

        [Test]
        public void CollectionChangedIsNotSuppressedAfterResumeEvents()
        {
            var sut = CreateSut();
            var raised = false;
            sut.CollectionChanged += (sender, args) => raised = true;

            sut.SuspendEvents();
            sut.ResumeEvents();

            RaiseCollectionChanged(sut);
            Assert.IsTrue(raised);
        }

        [Test]
        public void ResumeEventsRaisesCollectionChangedCorrectlyIfAddWasCalled()
        {
            var sut = CreateSut();
            sut.SuspendEvents();
            var node = NodeHelper.MockNode();
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NodeCollectionChangeAction.Add, args.Action);
                Assert.AreEqual(node, args.Element);
                raised = true;
            };

            sut.Add(node, 0);
            sut.ResumeEvents();
            Assert.IsTrue(raised);
        }

        [Test]
        public void SecondResumeEventsDoesNotRaiseCollectionChanged()
        {
            var sut = CreateSut();
            sut.SuspendEvents();
            RaiseCollectionChanged(sut);
            sut.ResumeEvents();

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            sut.ResumeEvents();
        }

        [Test]
        public void ResumeEventsDoesNotRaiseCollectionChangedIfNothingWasSuppressed()
        {
            var sut = CreateSut();
            sut.SuspendEvents();

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            sut.ResumeEvents();
        }

        [Test]
        public void ResumeEventsDoesNotRaiseCollectionChangedIfItAddWasCalledBeforSuspend()
        {
            var sut = CreateSut();
            RaiseCollectionChanged(sut);
            sut.SuspendEvents();

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            sut.ResumeEvents();
        }

        [Test]
        public void ResumeEventsRaisesCollectionChangedCorrectlyIfAddAndRemoveWereCalled()
        {
            var sut = CreateSut();
            sut.SuspendEvents();
            var node = NodeHelper.MockNode();
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NodeCollectionChangeAction.Reset, args.Action);
                Assert.IsNull(args.Element);
            };

            sut.Add(node, 0);
            sut.Remove(node);
            sut.ResumeEvents();
        }

        [Test]
        public void ResumeEventsRaisesCollectionChangedCorrectlyIfAddWasAlsoCalledBeforeLastResume()
        {
            var sut = CreateSut();
            sut.SuspendEvents();
            RaiseCollectionChanged(sut);
            sut.ResumeEvents();

            sut.SuspendEvents();
            var node = NodeHelper.MockNode();
            sut.Add(node, 0);
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NodeCollectionChangeAction.Add, args.Action);
                Assert.AreEqual(node, args.Element);
                raised = true;
            };

            sut.ResumeEvents();

            Assert.IsTrue(raised);
        }

        private static SuspendableNodeCollection<int> CreateSut() => new SuspendableNodeCollection<int>();

        private static void RaiseCollectionChanged(NodeCollection<int> sut)
        {
            sut.Add(NodeHelper.MockNode(), 0);
        }
    }
}