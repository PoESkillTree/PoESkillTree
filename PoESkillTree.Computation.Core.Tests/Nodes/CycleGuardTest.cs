using System;
using NUnit.Framework;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Nodes
{
    [TestFixture]
    public class CycleGuardTest
    {
        [Test]
        public void SutIsCycleGuard()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<ICycleGuard>(sut);
        }

        [Test]
        public void GuardThrowsOnSecondCall()
        {
            var sut = CreateSut();
            sut.Guard();

            Assert.Throws<InvalidOperationException>(() => sut.Guard());
        }

        [Test]
        public void GuardDoesNotThrowIfPreviousCallWasDisposed()
        {
            var sut = CreateSut();
            sut.Guard().Dispose();

            Assert.DoesNotThrow(() => sut.Guard());
        }

        private static CycleGuard CreateSut() => new CycleGuard();
    }
}