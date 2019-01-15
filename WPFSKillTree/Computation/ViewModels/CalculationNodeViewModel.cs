using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Computation.Model;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class CalculationNodeViewModel : Notifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CalculationNodeViewModel));

        private NodeValue? _value;
        private string _stringValue;

        public CalculationNodeViewModel(IStat stat, NodeType nodeType = NodeType.Total)
            => (Stat, NodeType) = (stat, nodeType);

        public IStat Stat { get; }
        public NodeType NodeType { get; }
        public Type DataType => Stat.DataType;
        public Array EnumValues => DataType.GetEnumValues();

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

        public NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value, ValueOnPropertyChanged);
        }

        private void ValueOnPropertyChanged()
        {
            OnPropertyChanged(nameof(NumericValue));
            OnPropertyChanged(nameof(BoolValue));
            StringValue = CalculateStringValue();
        }

        public double? NumericValue
        {
            get => Value?.Single;
            set => Value = (NodeValue?) value;
        }

        public bool BoolValue
        {
            get => Value.IsTrue();
            set => Value = (NodeValue?) value;
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
            return Value.ToString();
        }

        public IDisposable Observe(IObservableNodeRepository nodeRepository, IScheduler observeScheduler)
            => nodeRepository
                .ObserveNode(Stat, NodeType)
                .ObserveOn(observeScheduler)
                .Subscribe(
                    v => Value = v,
                    ex => Log.Error($"ObserveNode({Stat}, {NodeType}) failed", ex));
    }
}