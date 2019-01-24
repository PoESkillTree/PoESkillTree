using System;
using PoESkillTree.Computation.Common;

namespace POESKillTree.Computation.ViewModels
{
    public class GainOnActionStatViewModel : IDisposable
    {
        public GainOnActionStatViewModel(ResultNodeViewModel node)
            => Node = node;

        public ResultNodeViewModel Node { get; }

        public ExplicitRegistrationType.GainOnAction GainOnActionType
            => (ExplicitRegistrationType.GainOnAction) Node.Stat.ExplicitRegistrationType;

        public void Dispose()
            => Node?.Dispose();
    }
}