using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Model.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for the inventory. Only a collection of the the InventoryItemViewModels for the slots.
    /// </summary>
    public class InventoryViewModel : Notifier, IDisposable
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
        public IReadOnlyList<InventoryItemViewModel> TreeJewels { get; }

        public InventoryViewModel(
            IExtendedDialogCoordinator dialogCoordinator,
            ItemAttributes itemAttributes,
            IEnumerable<ushort> jewelPassiveNodes)
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
            Flasks = ItemSlotExtensions.Flasks.Select(s => CreateSlotVm(s)).ToList();
            TreeJewels = jewelPassiveNodes.Select(i => CreateSlotVm(ItemSlot.SkillTree, i)).ToList();
        }

        private InventoryItemViewModel CreateSlotVm(ItemSlot slot, ushort? socket = null)
        {
            var imageName = SlotToImageName(slot);
            return new InventoryItemViewModel(_dialogCoordinator, _itemAttributes, slot, socket)
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
                case ItemSlot.SkillTree:
                    return "Jewel";
                default:
                    return slot.ToString();
            }
        }

        public void Dispose()
        {
            Armor.Dispose();
            MainHand.Dispose();
            OffHand.Dispose();
            Ring.Dispose();
            Ring2.Dispose();
            Amulet.Dispose();
            Helm.Dispose();
            Gloves.Dispose();
            Boots.Dispose();
            Belt.Dispose();
            Flasks.ForEach(vm => vm.Dispose());
            TreeJewels.ForEach(vm => vm.Dispose());
        }
    }
}