using System;
using System.ComponentModel;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class GainOnActionStatViewModel : Notifier, IDisposable
    {
        public GainOnActionStatViewModel(ResultNodeViewModel node)
        {
            Node = node;
            Node.PropertyChanged += NodeOnPropertyChanged;
        }

        public ResultNodeViewModel Node { get; }

        public ExplicitRegistrationType.GainOnAction GainOnActionType
            => (ExplicitRegistrationType.GainOnAction) Node.Stat.ExplicitRegistrationType;

        public bool IsVisible
            => Node.Value.HasValue && GainOnActionType.GainedStat.Entity == Entity.Character;

        public string Action
            => GainOnActionType.ActionEntity == Entity.Character
                ? GainOnActionType.Action
                : $"{GainOnActionType.Action} by {GainOnActionType.ActionEntity}";

        public void Dispose()
        {
            Node.PropertyChanged -= NodeOnPropertyChanged;
            Node.Dispose();
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalculationNodeViewModel.Value))
                OnPropertyChanged(nameof(IsVisible));
        }
    }
}