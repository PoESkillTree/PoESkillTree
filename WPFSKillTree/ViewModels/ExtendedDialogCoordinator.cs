using System.Threading.Tasks;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Model;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
using PoESkillTree.ViewModels.Builds;
using PoESkillTree.ViewModels.Equipment;
using PoESkillTree.ViewModels.Skills;
using PoESkillTree.Views.Builds;
using PoESkillTree.Views.Equipment;
using PoESkillTree.Views.Skills;

namespace PoESkillTree.ViewModels
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
            return true;
        }

        public async Task EditSocketedGemsAsync(object context, ItemAttributes itemAttributes, ItemSlot itemSlot)
        {
            var skills = await _gameData.Skills;
            await ShowDialogAsync(context,
                new SkillsInSlotEditingViewModel(skills, _persistentData.EquipmentData.ItemImageService,
                    itemAttributes, itemSlot),
                new SkillsInSlotEditingDialog());
        }

        public async Task<TabPickerResult> EditStashTabAsync(object context, TabPickerViewModel tabPickerViewModel)
        {
            return await ShowDialogAsync(context, tabPickerViewModel, new TabPicker());
        }
    }
}