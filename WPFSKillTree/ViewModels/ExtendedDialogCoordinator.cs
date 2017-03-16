using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Builds;
using POESKillTree.ViewModels.Builds;
using POESKillTree.ViewModels.Equipment;
using POESKillTree.Views.Builds;
using POESKillTree.Views.Equipment;

namespace POESKillTree.ViewModels
{
    public interface IExtendedDialogCoordinator : IDialogCoordinator
    {
        Task<bool> EditBuildAsync(object context, IBuildViewModel<PoEBuild> buildVm, BuildValidator buildValidator);

        Task EditSocketedGemsAsync(object context, SocketedGemsEditingViewModel viewModel);

        Task<TabPickerResult> EditStashTabAsync(object context, TabPickerViewModel tabPickerViewModel);
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

        public async Task EditSocketedGemsAsync(object context, SocketedGemsEditingViewModel viewModel)
        {
            await ShowDialogAsync(context, viewModel, new SocketedGemsEditingView());
        }

        public async Task<TabPickerResult> EditStashTabAsync(object context, TabPickerViewModel tabPickerViewModel)
        {
            return await ShowDialogAsync(context, tabPickerViewModel, new TabPicker());
        }
    }
}