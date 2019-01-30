using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationNodeViewModel : CalculationNodeViewModel, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationNodeViewModel));

        private readonly Subject<NodeValue?> _valueChangeSubject = new Subject<NodeValue?>();

        public ConfigurationNodeViewModel(IStat stat) : base(stat)
        {
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(Value))
            {
                _valueChangeSubject.OnNext(Value);
            }
            base.OnPropertyChanged(propertyName);
        }

        public void SubscribeCalculator(IObservingCalculator calculator)
            => calculator.SubscribeTo(CreateValueObservable(),
                ex => Log.Error($"SubscribeCalculatorTo({Stat}) failed", ex));

        private IObservable<CalculatorUpdate> CreateValueObservable()
            => _valueChangeSubject
                .Select(p => CreateModifiers(Value))
                .Scan(CalculatorUpdate.Empty,
                    (u, ms) => new CalculatorUpdate(ms, u.AddedModifiers));

        private IReadOnlyList<Modifier> CreateModifiers(NodeValue? value)
        {
            if (value is null)
                return new Modifier[0];

            return new[]
            {
                new Modifier(new[] { Stat }, Form.TotalOverride,
                    new FunctionalValue(c => Calculate(c, value), $"{Stat.Minimum} <= {value} <= {Stat.Maximum}"),
                    new ModifierSource.Global(new ModifierSource.Local.UserSpecified()))
            };
        }

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
        {
            Value = null;
            _valueChangeSubject.OnCompleted();
            _valueChangeSubject.Dispose();
        }
    }
}