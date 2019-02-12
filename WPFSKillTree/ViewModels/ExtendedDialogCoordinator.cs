using System.Threading.Tasks;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.ViewModels.Builds;
using POESKillTree.ViewModels.Equipment;
using POESKillTree.Views.Builds;
using POESKillTree.Views.Equipment;

namespace POESKillTree.ViewModels
{
    public interface IExtendedDialogCoordinator : IDialogCoordinator
    {
        Task<bool> EditBuildAsync(object context, IBuildViewModel<PoEBuild> buildVm, BuildValidator buildValidator);

        Task EditSocketedGemsAsync(object context, ItemAttributes itemAttributes, ItemSlot itemSlot);

        Task<TabPickerResult> EditStashTabAsync(object context, TabPickerViewModel tabPickerViewModel);
    }

    public class ExtendedDialogCoordinator : DialogCoordinator, IExtendedDialogCoordinator
    {
        private readonly GameData _gameData;
        private readonly IPersistentData _persistentData;

        public ExtendedDialogCoordinator(GameData gameData, IPersistentData persistentData)
            => (_gameData, _persistentData) = (gameData, persistentData);

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

        public async Task EditSocketedGemsAsync(object context, ItemAttributes itemAttributes, ItemSlot itemSlot)
        {
            var skills = await _gameData.Skills;
            await ShowDialogAsync(context,
                new SocketedGemsEditingViewModel(skills, _persistentData.EquipmentData.ItemImageService,
                    itemAttributes, itemSlot),
                new SocketedGemsEditingView());
        }

        public async Task<TabPickerResult> EditStashTabAsync(object context, TabPickerViewModel tabPickerViewModel)
        {
            return await ShowDialogAsync(context, tabPickerViewModel, new TabPicker());
        }
    }
}