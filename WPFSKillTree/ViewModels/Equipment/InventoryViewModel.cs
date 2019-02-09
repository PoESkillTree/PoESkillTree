using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for the inventory. Only a collection of the the InventoryItemViewModels for the slots.
    /// </summary>
    public class InventoryViewModel : Notifier
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly ItemAttributes _itemAttributes;

        public InventoryItemViewModel Armor { get; }
        public InventoryItemViewModel MainHand { get; }
        public InventoryItemViewModel OffHand { get; }
        public InventoryItemViewModel Ring { get; }
        public InventoryItemViewModel Ring2 { get; }
        public InventoryItemViewModel Amulet { get; }
        public InventoryItemViewModel Helm { get; }
        public InventoryItemViewModel Gloves { get; }
        public InventoryItemViewModel Boots { get; }
        public InventoryItemViewModel Belt { get; }

        public InventoryViewModel(IExtendedDialogCoordinator dialogCoordinator, ItemAttributes itemAttributes)
        {
            _dialogCoordinator = dialogCoordinator;
            _itemAttributes = itemAttributes;
            Armor = CreateSlotVm(ItemSlot.BodyArmour);
            MainHand = CreateSlotVm(ItemSlot.MainHand);
            OffHand = CreateSlotVm(ItemSlot.OffHand);
            Ring = CreateSlotVm(ItemSlot.Ring);
            Ring2 = CreateSlotVm(ItemSlot.Ring2);
            Amulet = CreateSlotVm(ItemSlot.Amulet);
            Helm = CreateSlotVm(ItemSlot.Helm);
            Gloves = CreateSlotVm(ItemSlot.Gloves);
            Boots = CreateSlotVm(ItemSlot.Boots);
            Belt = CreateSlotVm(ItemSlot.Belt);
        }

        private InventoryItemViewModel CreateSlotVm(ItemSlot slot)
        {
            var imageName = slot.ToString();
            if (slot == ItemSlot.MainHand)
            {
                imageName = "TwoHandSword";
            }
            else if (slot == ItemSlot.OffHand)
            {
                imageName = "Shield";
            }
            else if (slot == ItemSlot.Ring2)
            {
                imageName = "Ring";
            }
            else if (slot == ItemSlot.Helm)
            {
                imageName = "Helmet";
            }

            return new InventoryItemViewModel(_dialogCoordinator, _itemAttributes, slot)
            {
                EmptyBackgroundImagePath = $"/POESKillTree;component/Images/EquipmentUI/ItemDefaults/{imageName}.png"
            };
        }
    }
}