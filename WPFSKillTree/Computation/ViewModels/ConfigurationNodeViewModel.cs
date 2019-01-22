using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationNodeViewModel : CalculationNodeViewModel, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationNodeViewModel));

        private IDisposable _subscription;

        public ConfigurationNodeViewModel(IStat stat) : base(stat)
        {
        }

        public void SubscribeCalculator(IObservingCalculator calculator)
            => _subscription = calculator.SubscribeTo(CreateValueObservable(),
                ex => Log.Error($"SubscribeCalculatorTo({Stat}) failed", ex));

        private IObservable<CalculatorUpdate> CreateValueObservable()
            => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => PropertyChanged += h,
                    h => PropertyChanged -= h)
                .Where(p => p.EventArgs.PropertyName == nameof(Value))
                .Select(p => CreateModifiers(Value))
                .Scan(CalculatorUpdate.Empty,
                    (u, ms) => new CalculatorUpdate(ms, u.AddedModifiers));

        private IReadOnlyList<Modifier> CreateModifiers(NodeValue? value)
            => new[]
            {
                new Modifier(new[] { Stat }, Form.TotalOverride,
                    new FunctionalValue(c => Calculate(c, value), $"{Stat.Minimum} <= {value} <= {Stat.Maximum}"),
                    new ModifierSource.Global(new ModifierSource.Local.UserSpecified()))
            };

        private NodeValue? Calculate(IValueCalculationContext context, NodeValue? nullableValue)
        {
            if (!(nullableValue is NodeValue value))
                return null;

            if (Stat.Minimum != null)
            {
                var minimum = context.GetValue(Stat.Minimum) ?? new NodeValue(double.MinValue);
                value = NodeValue.Combine(value, minimum, Math.Max);
            }

            if (Stat.Maximum != null)
            {
                var maximum = context.GetValue(Stat.Maximum) ?? new NodeValue(double.MaxValue);
                value = NodeValue.Combine(value, maximum, Math.Min);
            }

            return value;
        }

        public void Dispose()
            => _subscription?.Dispose();
    }
}