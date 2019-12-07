using System;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public sealed class ConfigurationStatViewModel : Notifier, IDisposable
    {
        public ConfigurationStatViewModel(
            ConfigurationNodeViewModel node,
            ResultNodeViewModel? minimumNode = null, ResultNodeViewModel? maximumNode = null)
        {
            Node = node;
            MinimumNode = minimumNode;
            MaximumNode = maximumNode;
        }

        public static ConfigurationStatViewModel Create(CalculationNodeViewModelFactory nodeFactory, IStat stat)
            => new ConfigurationStatViewModel(
                nodeFactory.CreateConfiguration(stat),
                stat.Minimum is null ? null : nodeFactory.CreateResult(stat.Minimum),
                stat.Maximum is null ? null : nodeFactory.CreateResult(stat.Maximum));

        public ConfigurationNodeViewModel Node { get; }
        public ResultNodeViewModel? MinimumNode { get; }
        public ResultNodeViewModel? MaximumNode { get; }

        public IStat Stat => Node.Stat;

        public void Dispose()
        {
            Node.Dispose();
            MinimumNode?.Dispose();
            MaximumNode?.Dispose();
        }
    }
}