using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using log4net;
using POESKillTree.Common.ViewModels;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ResultStatViewModel : Notifier, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResultStatViewModel));

        private readonly ModifierNodeViewModelFactory _modifierNodeFactory;
        private Lazy<NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>> _modifierNodes;

        public ResultStatViewModel(
            ResultNodeViewModel node, ModifierNodeViewModelFactory modifierNodeFactory,
            Action<ResultStatViewModel> removeAction)
        {
            _modifierNodeFactory = modifierNodeFactory;
            Node = node;
            Node.PropertyChanged += NodeOnPropertyChanged;
            RemoveCommand = new RelayCommand(() => removeAction(this));
            ResetModifierNodes();
        }

        public ResultNodeViewModel Node { get; }
        public NotifyingTask<IReadOnlyList<ModifierNodeViewModel>> ModifierNodes => _modifierNodes.Value;

        public ICommand RemoveCommand { get; }

        public void Dispose()
        {
            Node.PropertyChanged -= NodeOnPropertyChanged;
            Node.Dispose();
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ResultNodeViewModel.Value))
            {
                ResetModifierNodes();
                OnPropertyChanged(nameof(ModifierNodes));
            }
        }

        private void ResetModifierNodes()
        {
            _modifierNodes = new Lazy<NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>>(
                () => new NotifyingTask<IReadOnlyList<ModifierNodeViewModel>>(
                        _modifierNodeFactory.CreateAsync(Node.Stat, Node.NodeType),
                        ex => Log.Error($"Failed to create modifier nodes for {Node.Stat} {Node.NodeType}", ex))
                    { Default = new ModifierNodeViewModel[0] });
        }
    }
}