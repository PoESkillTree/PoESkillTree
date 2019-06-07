using System;
using System.Windows.Input;
using PoESkillTree.Utils;
using PoESkillTree.Common.ViewModels;

namespace PoESkillTree.Computation.ViewModels
{
    public class ResultStatViewModel : Notifier, IDisposable
    {
        public ResultStatViewModel(
            ResultNodeViewModel node, Action<ResultStatViewModel> removeAction)
        {
            Node = node;
            RemoveCommand = new RelayCommand(() => removeAction(this));
        }

        public ResultNodeViewModel Node { get; }

        public ICommand RemoveCommand { get; }

        public void Dispose()
        {
            Node.Dispose();
        }
    }
}