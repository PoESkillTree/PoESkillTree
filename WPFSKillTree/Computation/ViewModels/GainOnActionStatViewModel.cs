using System;
using System.ComponentModel;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public sealed class GainOnActionStatViewModel : Notifier, IDisposable
    {
        public GainOnActionStatViewModel(ResultNodeViewModel node)
        {
            if (node.Stat.ExplicitRegistrationType is null)
                throw new ArgumentException($"{nameof(node)}.Stat must be explicitly registered");
            Node = node;
            Node.PropertyChanged += NodeOnPropertyChanged;
        }

        public ResultNodeViewModel Node { get; }

        public ExplicitRegistrationType.GainOnAction GainOnActionType
            => (ExplicitRegistrationType.GainOnAction) Node.Stat.ExplicitRegistrationType!;

        public bool IsVisible
            => Node.HasValue && GainOnActionType.GainedStat.Entity == Entity.Character;

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
            if (e.PropertyName == nameof(ResultNodeViewModel.HasValue))
                OnPropertyChanged(nameof(IsVisible));
        }
    }
}