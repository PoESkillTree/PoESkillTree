using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model.Builds;
using POESKillTree.Views;

namespace POESKillTree.ViewModels
{
    public interface IExtendedDialogCoordinator : IDialogCoordinator
    {
        Task<bool> EditBuildAsync(object context, PoEBuild build);

        Task<bool> EditFolderAsync(object context, BuildFolder folder);
    }

    public class ExtendedDialogCoordinator : DialogCoordinator, IExtendedDialogCoordinator
    {
        public new static readonly IExtendedDialogCoordinator Instance = new ExtendedDialogCoordinator();

        public async Task<bool> EditBuildAsync(object context, PoEBuild build)
        {
            var vm = new EditBuildViewModel(build);
            if (!await ShowDialogAsync(context, vm, new EditBuildWindow()))
            {
                return false;
            }
            vm.Build.SaveToMemento().Restore(build);
            return true;
        }

        public async Task<bool> EditFolderAsync(object context, BuildFolder folder)
        {
            var name = await ShowInputAsync(context, L10n.Message("Edit Folder"),
                L10n.Message("Enter the new name for this folder below."),
                folder.Name);
            if (string.IsNullOrWhiteSpace(name))
                return false;
            folder.Name = name;
            return true;
        }
    }
}