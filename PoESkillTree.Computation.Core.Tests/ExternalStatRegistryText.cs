using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public partial class ExternalStatRegistryText
    {
        [Test]
        public void SutIsExternalStatRegistry()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IExternalStatRegistry>(sut);
        }

        [Test]
        public void RegisteredStatsIsEmptyInitially()
        {
            var sut = CreateSut();

            CollectionAssert.IsEmpty(sut.RegisteredStats);
        }

        [Test]
        public void RegisterAddsCorrectEntryToRegisteredStats()
        {
            var sut = CreateSut();

            KeyValuePair<IStat, double?> pair = Register(sut);

            Assert.That(sut.RegisteredStats, Has.Exactly(1).Items.EqualTo(pair));
        }

        [Test]
        public void UnregisterRemovesCorrectEntryFromRegisteredStats()
        {
            var sut = CreateSut();
            var (stat, _) = Register(sut);

            sut.Unregister(stat);

            CollectionAssert.IsEmpty(sut.RegisteredStats);
        }

        [Test]
        public void RegisterRaisesRegistryChanged()
        {
            var sut = CreateSut();
            var raised = false;
            var stat = Mock.Of<IStat>();
            var defaultValue = 42;
            sut.RegistryChanged += (sender, args) =>
            {
                Assert.AreSame(sut, sender);
                Assert.AreEqual(StatRegistryChangeType.Registered, args.ChangeType);
                Assert.AreEqual(stat, args.Stat);
                Assert.AreEqual(defaultValue, args.DefaultValue);
                raised = true;
            };

            sut.Register(stat, defaultValue);

            Assert.IsTrue(raised);
        }

        [Test]
        public void UnregisterRaisesRegistryChanged()
        {
            var sut = CreateSut();
            var (stat, defaultValue) = Register(sut);
            var raised = false;
            sut.RegistryChanged += (sender, args) =>
            {
                Assert.AreSame(sut, sender);
                Assert.AreEqual(StatRegistryChangeType.Unregistered, args.ChangeType);
                Assert.AreEqual(stat, args.Stat);
                Assert.AreEqual(defaultValue, args.DefaultValue);
                raised = true;
            };

            sut.Unregister(stat);

            Assert.IsTrue(raised);
        }

        [Test]
        public void UnregisterDoesNotRaiseRegistryChangedIfStatWasNotRegistered()
        {
            var sut = CreateSut();
            var stat = Mock.Of<IStat>();
            sut.RegistryChanged += (sender, args) => Assert.Fail();

            sut.Unregister(stat);
        }

        private static ExternalStatRegistry CreateSut()
        {
            return new ExternalStatRegistry();
        }

        private static KeyValuePair<IStat, double?> Register(IExternalStatRegistry sut)
        {
            var stat = new StatStub();
            var defaultValue = 42;

            sut.Register(stat, defaultValue);

            return new KeyValuePair<IStat, double?>(stat, defaultValue);
        }
    }
}