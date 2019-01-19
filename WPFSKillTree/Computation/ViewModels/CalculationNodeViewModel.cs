using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.GameModel;
using POESKillTree.Computation.Model;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class CalculationNodeViewModel : Notifier, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CalculationNodeViewModel));

        private NodeValue? _value;
        private string _stringValue;

        private IDisposable _subscription;

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
            return Value.ToString().Replace(" to ", " \nto ");
        }

        public void Observe(IObservableNodeRepository nodeRepository, IScheduler observeScheduler)
            => _subscription = nodeRepository
                .ObserveNode(Stat, NodeType)
                .ObserveOn(observeScheduler)
                .Subscribe(
                    v => Value = v,
                    ex => Log.Error($"ObserveNode({Stat}, {NodeType}) failed", ex));

        public void SubscribeCalculator(IObservingCalculator calculator)
            => _subscription = calculator.SubscribeTo(CreateValueObservable(),
                ex => Log.Error($"SubscribeCalculatorTo({Stat}) failed", ex));

        private IObservable<CalculatorUpdate> CreateValueObservable()
            => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => PropertyChanged += h,
                    h => PropertyChanged -= h)
                .Where(p => p.EventArgs.PropertyName == nameof(Value))
                .Select(p => CreateModifiers())
                .Scan(CalculatorUpdate.Empty,
                    (u, ms) => new CalculatorUpdate(ms, u.AddedModifiers));

        private IReadOnlyList<Modifier> CreateModifiers()
            => new[]
            {
                new Modifier(new[] { Stat }, Form.TotalOverride, new Constant(Value),
                    new ModifierSource.Global(new ModifierSource.Local.UserSpecified()))
            };

        public void Dispose()
            => _subscription?.Dispose();
    }
}