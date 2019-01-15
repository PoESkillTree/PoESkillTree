using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatViewModel : Notifier, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationStatViewModel));

        private IDisposable _subscriptions;

        public ConfigurationStatViewModel(IStat stat)
        {
            Node = new CalculationNodeViewModel(stat);
            if (stat.Minimum != null)
                MinimumNode = new CalculationNodeViewModel(stat.Minimum);
            if (stat.Maximum != null)
                MaximumNode = new CalculationNodeViewModel(stat.Maximum);
        }

        public CalculationNodeViewModel Node { get; }
        public CalculationNodeViewModel MinimumNode { get; }
        public CalculationNodeViewModel MaximumNode { get; }

        public IStat Stat => Node.Stat;
        public double DefaultMinimum => Node.DataType == typeof(uint) ? 0 : double.MinValue;
        public double DefaultMaximum => double.MaxValue;

        public void Observe(ObservableCalculator observableCalculator)
        {
            var subscriptions = new List<IDisposable>();
            if (MinimumNode != null)
                subscriptions.Add(MinimumNode.Observe(observableCalculator));
            if (MaximumNode != null)
                subscriptions.Add(MaximumNode.Observe(observableCalculator));
            subscriptions.Add(SubscribeNode(observableCalculator));
            _subscriptions = new CompositeDisposable(subscriptions);
        }

        private IDisposable SubscribeNode(ObservableCalculator observableCalculator)
        {
            var sub = observableCalculator.SubscribeCalculatorTo(CreateNodeValueObservable(),
                ex => Log.Error($"SubscribeCalculatorTo({Stat}) failed", ex));
            if (Stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue userSpecifiedValue)
            {
                Node.Value = userSpecifiedValue.DefaultValue;
            }
            return sub;
        }

        private IObservable<CalculatorUpdate> CreateNodeValueObservable()
            => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => Node.PropertyChanged += h,
                    h => Node.PropertyChanged -= h)
                .Where(p => p.EventArgs.PropertyName == nameof(CalculationNodeViewModel.Value))
                .Select(p => CreateModifiers((CalculationNodeViewModel) p.Sender))
                .Scan(new CalculatorUpdate(new Modifier[0], new Modifier[0]),
                    (u, ms) => new CalculatorUpdate(ms, u.AddedModifiers));

        private static IReadOnlyList<Modifier> CreateModifiers(CalculationNodeViewModel node)
            => new[]
            {
                new Modifier(new[] { node.Stat }, Form.TotalOverride, new Constant(node.Value),
                    new ModifierSource.Global(new ModifierSource.Local.UserSpecified()))
            };

        public void Dispose()
            => _subscriptions?.Dispose();
    }
}