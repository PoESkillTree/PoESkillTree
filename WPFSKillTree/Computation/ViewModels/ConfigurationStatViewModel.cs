using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.GameModel;
using POESKillTree.Computation.Model;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatViewModel : Notifier, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationStatViewModel));

        private readonly Subject<CalculatorUpdate> _updateSubject = new Subject<CalculatorUpdate>();

        private IDisposable _subscriptions;

        private NodeValue? _value;
        private NodeValue? _minimum;
        private NodeValue? _maximum;

        public ConfigurationStatViewModel(IStat stat)
            => Stat = stat;

        public IStat Stat { get; }
        public string Name => ToString();
        public Array EnumValues => Stat.DataType.GetEnumValues();

        private NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value, ValueOnPropertyChanged, ValueOnPropertyChanging);
        }

        private void ValueOnPropertyChanging(NodeValue? newValue)
        {
            if (!_updateSubject.IsDisposed)
            {
                _updateSubject.OnNext(CreateCalculatorUpdate(_value, newValue));
            }
        }

        private void ValueOnPropertyChanged()
        {
            OnPropertyChanged(nameof(NumericValue));
            OnPropertyChanged(nameof(BoolValue));
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

        private NodeValue? Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value, () => OnPropertyChanged(nameof(NumericMinimum)));
        }

        public double NumericMinimum => Minimum?.Single ?? double.MinValue;

        private NodeValue? Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value, () => OnPropertyChanged(nameof(NumericMaximum)));
        }

        public double NumericMaximum => Maximum?.Single ?? double.MaxValue;

        public void Observe(ObservableCalculator observableCalculator, ICalculationNode node)
        {
            if (!(Stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue userSpecifiedValue))
                throw new ArgumentException("Configuration stats must be UserDefinedValue");

            Value = userSpecifiedValue.DefaultValue;

            var minSubscription = observableCalculator.ObserveNode(Stat.Minimum)
                .ObserveOnDispatcher()
                .Subscribe(v => Minimum = v,
                    ex => Log.Error($"ObserveNode({Stat.Minimum}) failed", ex));
            var maxSubscription = observableCalculator.ObserveNode(Stat.Maximum)
                .ObserveOnDispatcher()
                .Subscribe(v => Maximum = v,
                    ex => Log.Error($"ObserveNode({Stat.Maximum}) failed", ex));

            var calculatorSubscription = observableCalculator.SubscribeCalculatorTo(
                _updateSubject.AsObservable(),
                ex => Log.Error($"SubscribeCalculatorTo({Stat}) failed", ex));
            _updateSubject.OnNext(new CalculatorUpdate(new[] { CreateModifier(Value) }, new Modifier[0]));

            _subscriptions = new CompositeDisposable(minSubscription, maxSubscription, calculatorSubscription);
        }

        private CalculatorUpdate CreateCalculatorUpdate(NodeValue? oldValue, NodeValue? newValue)
            => new CalculatorUpdate(new[] { CreateModifier(newValue) }, new[] { CreateModifier(oldValue) });

        private Modifier CreateModifier(NodeValue? value)
            => new Modifier(new[] { Stat }, Form.TotalOverride, new Constant(value),
                new ModifierSource.Global(new ModifierSource.Local.UserDefined()));

        public void Dispose()
        {
            if (!_updateSubject.IsDisposed)
            {
                _updateSubject.OnNext(new CalculatorUpdate(new Modifier[0], new[] { CreateModifier(Value) }));
                _updateSubject.OnCompleted();
            }
            _subscriptions?.Dispose();
            _updateSubject.Dispose();
        }

        public override string ToString()
        {
            var result = Stat.Identity;
            if (Stat.Entity != Entity.Character)
            {
                result = $"{Stat.Entity} {result}";
            }
            return result;
        }
    }
}