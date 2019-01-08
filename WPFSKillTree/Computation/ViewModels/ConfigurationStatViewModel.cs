using System;
using System.Collections.Generic;
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

        private readonly Subject<IReadOnlyList<Modifier>> _updateSubject = new Subject<IReadOnlyList<Modifier>>();

        private IDisposable _subscriptions;

        private NodeValue? _value;
        private NodeValue? _minimum;
        private NodeValue? _maximum;

        public ConfigurationStatViewModel(IStat stat)
            => Stat = stat;

        public IStat Stat { get; }
        public string Name => ToString();
        public Array EnumValues => Stat.DataType.GetEnumValues();

        public NodeValue? Value
        {
            get => _value;
            set => SetProperty(ref _value, value, ValueOnPropertyChanged);
        }

        private void ValueOnPropertyChanged()
        {
            if (!_updateSubject.IsDisposed)
            {
                _updateSubject.OnNext(CreateModifiers(Value));
            }
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

        public void Observe(ObservableCalculator observableCalculator)
        {
            var minSubscription = observableCalculator.ObserveNode(Stat.Minimum)
                .ObserveOnDispatcher()
                .Subscribe(v => Minimum = v,
                    ex => Log.Error($"ObserveNode({Stat.Minimum}) failed", ex));
            var maxSubscription = observableCalculator.ObserveNode(Stat.Maximum)
                .ObserveOnDispatcher()
                .Subscribe(v => Maximum = v,
                    ex => Log.Error($"ObserveNode({Stat.Maximum}) failed", ex));

            var calculatorSubscription = observableCalculator.SubscribeCalculatorTo(
                _updateSubject.AsObservable()
                    .Scan(new CalculatorUpdate(new Modifier[0], new Modifier[0]),
                        (u, ms) => new CalculatorUpdate(ms, u.AddedModifiers)),
                ex => Log.Error($"SubscribeCalculatorTo({Stat}) failed", ex));
            if (Stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue userSpecifiedValue)
            {
                Value = userSpecifiedValue.DefaultValue;
            }

            _subscriptions = new CompositeDisposable(minSubscription, maxSubscription, calculatorSubscription);
        }

        private IReadOnlyList<Modifier> CreateModifiers(NodeValue? value)
            => new[]
            {
                new Modifier(new[] { Stat }, Form.TotalOverride, new Constant(value),
                    new ModifierSource.Global(new ModifierSource.Local.UserDefined()))
            };

        public void Dispose()
        {
            if (!_updateSubject.IsDisposed)
            {
                _updateSubject.OnNext(new Modifier[0]);
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