using System;
using System.ComponentModel;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public sealed class PassiveTreeConnectionViewModel : Notifier, IDisposable
    {
        public PassiveTreeConnectionViewModel(ResultNodeViewModel node)
        {
            if (!(node.Stat.ExplicitRegistrationType is ExplicitRegistrationType.PassiveTreeConnection))
                throw new ArgumentException($"{nameof(node)}.Stat must be explicitly registered");
            Node = node;
            Node.PropertyChanged += NodeOnPropertyChanged;
        }

        public ResultNodeViewModel Node { get; }

        public bool Connected => Node.BoolValue;

        public ushort SourceNode => ((ExplicitRegistrationType.PassiveTreeConnection) Node.Stat.ExplicitRegistrationType!).SourceNode;
        public ushort TargetNode => ((ExplicitRegistrationType.PassiveTreeConnection) Node.Stat.ExplicitRegistrationType!).TargetNode;

        public void Dispose()
        {
            Node.PropertyChanged -= NodeOnPropertyChanged;
            Node.Dispose();
        }

        private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ResultNodeViewModel.BoolValue))
                OnPropertyChanged(nameof(Connected));
        }
    }
}