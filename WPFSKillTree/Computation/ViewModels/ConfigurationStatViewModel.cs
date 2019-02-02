using System;
using PoESkillTree.Computation.Common;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatViewModel : Notifier, IDisposable
    {
        public ConfigurationStatViewModel(CalculationNodeViewModelFactory nodeFactory, IStat stat)
        {
            Node = nodeFactory.CreateConfiguration(stat);
            if (stat.Minimum != null)
                MinimumNode = nodeFactory.CreateResult(stat.Minimum);
            if (stat.Maximum != null)
                MaximumNode = nodeFactory.CreateResult(stat.Maximum);
        }

        public ConfigurationNodeViewModel Node { get; }
        public ResultNodeViewModel MinimumNode { get; }
        public ResultNodeViewModel MaximumNode { get; }

        public IStat Stat => Node.Stat;

        public void Dispose()
        {
            Node.Dispose();
            MinimumNode?.Dispose();
            MaximumNode?.Dispose();
        }
    }
}