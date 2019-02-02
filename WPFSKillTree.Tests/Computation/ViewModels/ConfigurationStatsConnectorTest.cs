using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using POESKillTree.Computation.ViewModels;
using POESKillTree.Model;
using POESKillTree.Model.Builds;

namespace PoESkillTree.Tests.Computation.ViewModels
{
    [TestFixture]
    public class ConfigurationStatsConnectorTest
    {
        [Test]
        public void InitializeSetsNodeValuesCorrectly()
        {
            var stats = new[] { new Stat("a"), new Stat("b"), };
            var expected = new NodeValue?[] { new NodeValue(42), null };
            var build = new PoEBuild();
            build.ConfigurationStats.SetValue(stats[0], expected[0]);
            var nodes = stats.Select(s => new ConfigurationNodeViewModel(s)).ToArray();
            SetupSut(build, nodes);

            Assert.AreEqual(expected, nodes.Select(n => n.Value));
        }

        [Test]
        public void ListensToNodeValueChanges()
        {
            var stat = new Stat("a");
            var expected = new NodeValue(42);
            var build = new PoEBuild();
            var node = new ConfigurationNodeViewModel(stat);
            SetupSut(build, node);

            node.Value = expected;

            Assert.IsTrue(build.ConfigurationStats.TryGetValue(stat, out var actual));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ListensToNodeCollectionChanges()
        {
            var stats = new[] { new Stat("a"), new Stat("b"), };
            var expected = new (string, double?)[] { ("a", 42), ("b", 43) };
            var build = CreateBuild(expected);
            var statViewModels = stats.Select(s => new ConfigurationNodeViewModel(s))
                .Select(n => new ConfigurationStatViewModel(n)).ToList();
            var observableCollection = new ObservableCollection<ConfigurationStatViewModel> { statViewModels[0] };
            SetupSut(build, observableCollection);

            observableCollection.Remove(statViewModels[0]);
            observableCollection.Add(statViewModels[1]);
            statViewModels[0].Node.ResetValue();

            Assert.AreEqual(expected, build.ConfigurationStats.Export());
        }

        [Test]
        public void ListensToPoEBuildChanges()
        {
            var stat = new Stat("a");
            var expected = new NodeValue(42);
            var build = new PoEBuild();
            build.ConfigurationStats.SetValue(stat, expected);
            var node = new ConfigurationNodeViewModel(stat);
            SetupSut(build, node);
            node.Value = null;

            build.RevertChanges();

            Assert.AreEqual(expected, node.Value);
        }

        [Test]
        public void ListensToPersistentDataChanges()
        {
            var stat = new Stat("a");
            var expected = new NodeValue(42);
            var oldBuild = new PoEBuild();
            var build = new PoEBuild();
            build.ConfigurationStats.SetValue(stat, expected);
            var persistentDataMock = new Mock<IPersistentData>();
            persistentDataMock.Setup(d => d.CurrentBuild).Returns(oldBuild);
            var node = new ConfigurationNodeViewModel(stat);
            SetupSut(persistentDataMock.Object, node);

            persistentDataMock.Raise(d => d.PropertyChanging += null,
                new PropertyChangingEventArgs(nameof(IPersistentData.CurrentBuild)));
            persistentDataMock.Setup(d => d.CurrentBuild).Returns(build);
            persistentDataMock.Raise(d => d.PropertyChanged += null,
                new PropertyChangedEventArgs(nameof(IPersistentData.CurrentBuild)));

            Assert.AreEqual(expected, node.Value);
        }

        private static void SetupSut(PoEBuild build, params ConfigurationNodeViewModel[] nodes)
        {
            var persistentData = Mock.Of<IPersistentData>(d => d.CurrentBuild == build);
            SetupSut(persistentData, nodes);
        }

        private static void SetupSut(IPersistentData persistentData, params ConfigurationNodeViewModel[] nodes)
        {
            persistentData.CurrentBuild.KeepChanges();
            var viewModels = Mock.Of<INotifyCollectionChanged>();
            var sut = new ConfigurationStatsConnector(persistentData, nodes);
            sut.Initialize(viewModels);
        }

        private static void SetupSut(PoEBuild build, ObservableCollection<ConfigurationStatViewModel> statViewModels)
        {
            build.KeepChanges();
            var persistentData = Mock.Of<IPersistentData>(d => d.CurrentBuild == build);
            ConfigurationStatsConnector.Connect(persistentData, statViewModels);
        }

        private static PoEBuild CreateBuild(IEnumerable<(string, double?)> configurationStats)
            => new PoEBuild(null, new string[0][], new ushort[0], new ushort[0], configurationStats, null);
    }
}