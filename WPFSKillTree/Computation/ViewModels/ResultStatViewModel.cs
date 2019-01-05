using System;
using System.Reactive.Linq;
using System.Windows.Input;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Common.ViewModels;
using POESKillTree.Computation.Model;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatViewModel : Notifier, IDisposable
    {
        private string _value;
        private IDisposable _subscription;

        public ResultStatViewModel(IStat stat, NodeType nodeType, Action<ResultStatViewModel> removeAction)
        {
            (Stat, NodeType) = (stat, nodeType);
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public void Connect(ObservableCalculator observableCalculator)
            => _subscription = observableCalculator
                .ObserveNode(Stat, NodeType)
                .ObserveOnDispatcher()
                .Subscribe(SetValue);

        public IStat Stat { get; }
        public string Name => ToString();

        public NodeType NodeType { get; }

        public string Value
        {
            get => _value;
            private set => SetProperty(ref _value, value);
        }

        public ICommand RemoveCommand { get; }

        public void Dispose()
            => _subscription?.Dispose();

        private void SetValue(NodeValue? value)
        {
            var dataType = Stat.DataType;
            if (dataType == typeof(int) || dataType == typeof(double))
            {
                Value = value?.ToString() ?? L10n.Message("None");
            }
            else if (dataType == typeof(bool))
            {
                Value = value.IsTrue().ToString();
            }
            else if (dataType.IsEnum)
            {
                Value = value is null
                    ? L10n.Message("None")
                    : dataType.GetEnumValues().GetValue((int) value.Single()).ToString();
            }
            else
            {
                Value = null;
            }
        }

        public override string ToString()
        {
            var result = Stat.Identity;
            if (Stat.Entity != Entity.Character)
            {
                result = $"{Stat.Entity} {result}";
            }
            if (NodeType != NodeType.Total)
            {
                result = $"{result} ({NodeType})";
            }
            return result;
        }
    }
}