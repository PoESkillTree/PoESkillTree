using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using log4net;
using PoESkillTree.Computation.Common;
using POESKillTree.Localization;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultNodeViewModel : CalculationNodeViewModel, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultNodeViewModel));

        private string _stringValue;

        private IDisposable _subscription;

        public ResultNodeViewModel(IStat stat, NodeType nodeType = NodeType.Total) : base(stat, nodeType)
        {
        }

        public string StringValue
        {
            get => _stringValue ?? (_stringValue = CalculateStringValue());
            private set => SetProperty(ref _stringValue, value);
        }

        private string CalculateStringValue()
        {
            if (DataType == typeof(bool))
                return Value.IsTrue().ToString();
            if (Value is null)
                return L10n.Message("None");
            if (DataType.IsEnum)
                return EnumValues.GetValue((int) Value.Single()).ToString();
            return Value.Select(d => Math.Round(d, 2, MidpointRounding.AwayFromZero))
                .ToString().Replace(" to ", " \nto ");
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(Value))
            {
                StringValue = CalculateStringValue();
            }
            base.OnPropertyChanged(propertyName);
        }

        public void Observe(IObservable<NodeValue?> observable, IScheduler observeScheduler)
            => _subscription = observable
                .ObserveOn(observeScheduler)
                .Subscribe(
                    v => Value = v,
                    ex => Log.Error($"ObserveNode({Stat}, {NodeType}) failed", ex));

        public void Dispose()
            => _subscription?.Dispose();
    }
}