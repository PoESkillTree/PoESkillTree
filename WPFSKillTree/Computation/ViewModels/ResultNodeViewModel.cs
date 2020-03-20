using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NLog;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public sealed class ResultNodeViewModel : CalculationNodeViewModel, IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ModifierNodeViewModelFactory _modifierNodeFactory;
        private Lazy<NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>> _modifierNodes;

        private IDisposable? _subscription;

        public ResultNodeViewModel(
            ModifierNodeViewModelFactory modifierNodeFactory, IStat stat, NodeType nodeType = NodeType.Total)
            : base(stat, nodeType)
        {
            _modifierNodeFactory = modifierNodeFactory;
            _modifierNodes = CreateModifierNodes();
        }

        public NotifyingTask<IReadOnlyList<ModifierNodeViewModel>> ModifierNodes => _modifierNodes.Value;

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(Value))
            {
                _modifierNodes = CreateModifierNodes();
                base.OnPropertyChanged(nameof(ModifierNodes));
            }
            base.OnPropertyChanged(propertyName);
        }

        public void Observe(IObservable<NodeValue?> observable, IScheduler observeScheduler)
            => _subscription = observable
                .ObserveOn(observeScheduler)
                .Subscribe(
                    v => Value = v,
                    ex => Log.Error(ex, $"ObserveNode({Stat}, {NodeType}) failed"));

        public void Dispose()
            => _subscription?.Dispose();

        private Lazy<NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>> CreateModifierNodes()
            => new Lazy<NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>>(
                () => new NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>(
                        _modifierNodeFactory.CreateAsync(Stat, NodeType),
                        Array.Empty<ModifierNodeViewModel>(),
                        ex => Log.Error(ex, $"Failed to create modifier nodes for {Stat} {NodeType}")));
    }
}