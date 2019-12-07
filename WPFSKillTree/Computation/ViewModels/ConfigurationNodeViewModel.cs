using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;

namespace PoESkillTree.Computation.ViewModels
{
    public sealed class ConfigurationNodeViewModel : CalculationNodeViewModel, IDisposable
    {
        private readonly NodeValue? _defaultValue;
        private readonly Subject<NodeValue?> _valueChangeSubject = new Subject<NodeValue?>();

        public ConfigurationNodeViewModel(IStat stat, NodeValue? defaultValue = null) : base(stat)
        {
            _defaultValue = defaultValue;
        }

        public void ResetValue()
        {
            Value = Stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue userSpecifiedValue
                ? userSpecifiedValue.DefaultValue
                : _defaultValue;
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
            => calculator.SubscribeTo(CreateValueObservable());

        private IObservable<CalculatorUpdate> CreateValueObservable()
            => _valueChangeSubject
                .Select(p => CreateModifiers(Value))
                .Scan(CalculatorUpdate.Empty,
                    (u, ms) => new CalculatorUpdate(ms, u.AddedModifiers))
                .Where(u => !u.AddedModifiers.SequenceEqual(u.RemovedModifiers));

        private IReadOnlyList<Modifier> CreateModifiers(NodeValue? value)
        {
            if (value is null)
                return Array.Empty<Modifier>();

            return new[]
            {
                new Modifier(new[] { Stat }, Form.TotalOverride,
                    new FunctionalValue(c => Calculate(c, value.Value),
                        $"{Stat.Minimum} <= {value} <= {Stat.Maximum}"),
                    new ModifierSource.Global(new ModifierSource.Local.UserSpecified()))
            };
        }

        private NodeValue? Calculate(IValueCalculationContext context, NodeValue value)
        {
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