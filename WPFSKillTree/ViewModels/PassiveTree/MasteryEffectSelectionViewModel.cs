using PoESkillTree.Common.ViewModels;
using PoESkillTree.Utils.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class MasteryEffectSelectionViewModelProxy : BindingProxy<MasteryEffectSelectionViewModel>
    {
    }

    /// <summary>
    /// View model for selecting a mastery effect.
    /// </summary>
    public class MasteryEffectSelectionViewModel : CloseableViewModel<bool>
    {
        

        public PassiveNodeViewModel Node { get; }
        public IEnumerable<PassiveNodeViewModel> Masteries { get; }

        public MasteryEffectSelectionViewModel(PassiveNodeViewModel node, IEnumerable<PassiveNodeViewModel> masteries)
        {
            Node = node;
            Masteries = masteries;
        }

        public bool IsEffectEnabled(ushort effect) => Node.Skill == effect || !Masteries.Any(x => x.Skill == effect);
        public bool IsEffectChecked(ushort effect) => Node.Skill == effect || Masteries.Any(x => x.Skill == effect);

        public ICommand SetEffect => new RelayCommand<ushort>((effect) =>
        {
            Node.Skill = effect;
        });

    }
}
