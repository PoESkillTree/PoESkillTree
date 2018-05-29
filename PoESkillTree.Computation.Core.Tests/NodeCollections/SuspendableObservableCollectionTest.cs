using System.ComponentModel;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;

namespace PoESkillTree.Computation.Core.Tests.NodeCollections
{
    [TestFixture]
    public class SuspendableObservableCollectionTest
    {
        [Test]
        public void SutIsObservableCollection()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ObservableCollection<int>>(sut);
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
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(CollectionChangeAction.Add, args.Action);
                Assert.AreEqual(0, args.Element);
                raised = true;
            };

            sut.Add(0);
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
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(CollectionChangeAction.Refresh, args.Action);
                Assert.IsNull(args.Element);
            };

            sut.Add(0);
            sut.Remove(0);
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
            sut.Add(0);
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(CollectionChangeAction.Add, args.Action);
                Assert.AreEqual(0, args.Element);
                raised = true;
            };

            sut.ResumeEvents();

            Assert.IsTrue(raised);
        }

        private static SuspendableObservableCollection<int> CreateSut() => new SuspendableObservableCollection<int>();

        private static void RaiseCollectionChanged(SuspendableObservableCollection<int> sut)
        {
            sut.Add(-1);
        }
    }
}