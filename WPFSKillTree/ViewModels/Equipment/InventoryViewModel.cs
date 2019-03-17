using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.Utils;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Equipment
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
        public IReadOnlyList<InventoryItemViewModel> Flasks { get; }

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
            Flasks = ItemSlotExtensions.Flasks.Select(CreateSlotVm).ToList();
        }

        private InventoryItemViewModel CreateSlotVm(ItemSlot slot)
        {
            var imageName = SlotToImageName(slot);
            return new InventoryItemViewModel(_dialogCoordinator, _itemAttributes, slot)
            {
                EmptyBackgroundImagePath = $"/PoESkillTree;component/Images/EquipmentUI/ItemDefaults/{imageName}.png"
            };
        }

        private static string SlotToImageName(ItemSlot slot)
        {
            if (slot.IsFlask())
                return "LifeFlask";
            switch (slot)
            {
                case ItemSlot.MainHand:
                    return "TwoHandSword";
                case ItemSlot.OffHand:
                    return "Shield";
                case ItemSlot.Ring2:
                    return "Ring";
                case ItemSlot.Helm:
                    return "Helmet";
                default:
                    return slot.ToString();
            }
        }
    }
}