using System;
using System.Reactive.Linq;
using System.Windows.Input;
using log4net;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultStatViewModel));

        private string _value;
        private IDisposable _subscription;

        public ResultStatViewModel(IStat stat, NodeType nodeType, Action<ResultStatViewModel> removeAction)
        {
            (Stat, NodeType) = (stat, nodeType);
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public IStat Stat { get; }
        public string Name => ToString();

        public NodeType NodeType { get; }

        public string Value
        {
            get => _value;
            private set => SetProperty(ref _value, value);
        }

        public ICommand RemoveCommand { get; }

        public void Observe(ObservableCalculator observableCalculator)
            => _subscription = observableCalculator
                .ObserveNode(Stat, NodeType)
                .ObserveOnDispatcher()
                .Subscribe(OnNext, OnError);

        public void Dispose()
            => _subscription?.Dispose();

        private void OnNext(NodeValue? value)
        {
            var dataType = Stat.DataType;
            if (dataType == typeof(bool))
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
                Value = value?.ToString() ?? L10n.Message("None");
            }
        }

        private void OnError(Exception exception)
        {
            Log.Error($"ObserveNode({Stat}, {NodeType}) failed", exception);
            Value = L10n.Message("Error");
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