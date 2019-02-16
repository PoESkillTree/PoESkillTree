using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using POESKillTree.Computation.Model;

namespace PoESkillTree.Tests.Computation.Model
{
    [TestFixture]
    public class ExplicitlyRegisteredStatsObserverTest
    {
        [Test]
        public void InvokesAddOnAdd()
        {
            var expected = new Stat("");
            var actualAdded = new List<IStat>();
            var actualRemoved = new List<IStat>();
            var nodeCollection = CreateNodeCollection();
            SetupSut(nodeCollection, actualAdded, actualRemoved);

            nodeCollection.Add(null, expected);

            Assert.That(actualAdded, Has.One.EqualTo(expected));
            Assert.IsEmpty(actualRemoved);
        }

        [Test]
        public void InvokesRemoveOnRemove()
        {
            var expected = new Stat("");
            var actualAdded = new List<IStat>();
            var actualRemoved = new List<IStat>();
            var nodeCollection = CreateNodeCollection();
            SetupSut(nodeCollection, actualAdded, actualRemoved);
            nodeCollection.Add(null, expected);
            actualAdded.Clear();

            nodeCollection.Remove(null, expected);
            
            Assert.IsEmpty(actualAdded);
            Assert.That(actualRemoved, Has.One.EqualTo(expected));
        }

        [Test]
        public void InvokesAddAndRemoveOnRefresh()
        {
            var stats = new[] { new Stat("a"), new Stat("b"), new Stat("c"), };
            var actualAdded = new List<IStat>();
            var actualRemoved = new List<IStat>();
            var eventBuffer = new EventBuffer();
            var nodeCollection = CreateNodeCollection(eventBuffer);
            SetupSut(nodeCollection, actualAdded, actualRemoved);
            nodeCollection.Add(null, stats[0]);
            nodeCollection.Add(null, stats[1]);
            actualAdded.Clear();

            eventBuffer.StartBuffering();
            nodeCollection.Add(null, stats[2]);
            nodeCollection.Remove(null, stats[0]);
            eventBuffer.Flush();

            Assert.AreEqual(stats.Skip(1), actualAdded);
            Assert.AreEqual(stats.Take(2), actualRemoved);
        }

        [Test]
        public void RefreshesOnInitialize()
        {
            var expected = new Stat("");
            var actualAdded = new List<IStat>();
            var actualRemoved = new List<IStat>();
            var nodeCollection = CreateNodeCollection();
            nodeCollection.Add(null, expected);

            SetupSut(nodeCollection, actualAdded, actualRemoved);

            Assert.That(actualAdded, Has.One.EqualTo(expected));
            Assert.IsEmpty(actualRemoved);
        }

        private static void SetupSut(
            NodeCollection<IStat> nodeCollection, ICollection<IStat> addedStats, ICollection<IStat> removedStats)
        {
            var observableCalculator = new ObservableCalculator(
                Mock.Of<ICalculator>(c => c.ExplicitlyRegisteredStats == nodeCollection),
                ImmediateScheduler.Instance);
            var sut = new ExplicitlyRegisteredStatsObserver(observableCalculator);
            sut.StatAdded += (_, s) => addedStats.Add(s);
            sut.StatRemoved += (_, s) => removedStats.Add(s);
            sut.InitializeAsync(ImmediateScheduler.Instance).GetAwaiter().GetResult();
        }

        private static NodeCollection<IStat> CreateNodeCollection(IEventBuffer eventBuffer = null)
            => new NodeCollection<IStat>(eventBuffer ?? new EventBuffer());
    }
}