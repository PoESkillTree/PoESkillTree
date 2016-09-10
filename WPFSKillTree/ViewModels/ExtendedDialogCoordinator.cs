using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Builds;
using POESKillTree.ViewModels.Builds;
using POESKillTree.Views;

namespace POESKillTree.ViewModels
{
    public interface IExtendedDialogCoordinator : IDialogCoordinator
    {
        Task<bool> EditBuildAsync(object context, IBuildViewModel<PoEBuild> buildVm, BuildValidator buildValidator);
    }

    public class ExtendedDialogCoordinator : DialogCoordinator, IExtendedDialogCoordinator
    {
        public new static readonly IExtendedDialogCoordinator Instance = new ExtendedDialogCoordinator();

        public async Task<bool> EditBuildAsync(object context, IBuildViewModel<PoEBuild> buildVm,
            BuildValidator buildValidator)
        {
            var vm = new EditBuildViewModel(buildVm, buildValidator);
            if (!await ShowDialogAsync(context, vm, new EditBuildWindow()))
            {
                return false;
            }
            var build = buildVm.Build;
            build.Name = vm.Name;
            build.Note = vm.Note;
            build.AccountName = vm.AccountName;
            build.CharacterName = vm.CharacterName;
            return true;
        }
    }
}