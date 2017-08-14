using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for the inventory. Only a collection of the the InventoryItemViewModels for the slots.
    /// </summary>
    public class InventoryViewModel : Notifier
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly EquipmentData _equipmentData;
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

        //Jewel Slots
        public InventoryItemViewModel Jewel { get; }
        public InventoryItemViewModel Jewel02 { get; }
        public InventoryItemViewModel Jewel03 { get; }
        public InventoryItemViewModel Jewel04 { get; }
        public InventoryItemViewModel Jewel05 { get; }
        public InventoryItemViewModel Jewel06 { get; }
        public InventoryItemViewModel Jewel07 { get; }
        public InventoryItemViewModel Jewel08 { get; }
        public InventoryItemViewModel Jewel09 { get; }
        public InventoryItemViewModel Jewel10 { get; }
        public InventoryItemViewModel Jewel11 { get; }
        public InventoryItemViewModel Jewel12 { get; }
        public InventoryItemViewModel Jewel13 { get; }
        public InventoryItemViewModel Jewel14 { get; }
        public InventoryItemViewModel Jewel15 { get; }
        public InventoryItemViewModel Jewel16 { get; }
        public InventoryItemViewModel Jewel17 { get; }
        public InventoryItemViewModel Jewel18 { get; }
        public InventoryItemViewModel Jewel19 { get; }
        public InventoryItemViewModel Jewel20 { get; }
        public InventoryItemViewModel Jewel21 { get; }

        public InventoryViewModel(IExtendedDialogCoordinator dialogCoordinator, EquipmentData equipmentData,
            ItemAttributes itemAttributes)
        {
            _dialogCoordinator = dialogCoordinator;
            _equipmentData = equipmentData;
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

            //Jewel Slots
            Jewel = CreateSlotVm(ItemSlot.Jewel);
            Jewel02 = CreateSlotVm(ItemSlot.Jewel02);
            Jewel03 = CreateSlotVm(ItemSlot.Jewel03);
            Jewel04 = CreateSlotVm(ItemSlot.Jewel04);
            Jewel05 = CreateSlotVm(ItemSlot.Jewel05);
            Jewel06 = CreateSlotVm(ItemSlot.Jewel06);
            Jewel07 = CreateSlotVm(ItemSlot.Jewel07);
            Jewel08 = CreateSlotVm(ItemSlot.Jewel08);
            Jewel09 = CreateSlotVm(ItemSlot.Jewel09);
            Jewel10 = CreateSlotVm(ItemSlot.Jewel10);
            Jewel11 = CreateSlotVm(ItemSlot.Jewel11);
            Jewel12 = CreateSlotVm(ItemSlot.Jewel12);
            Jewel13 = CreateSlotVm(ItemSlot.Jewel13);
            Jewel14 = CreateSlotVm(ItemSlot.Jewel14);
            Jewel15 = CreateSlotVm(ItemSlot.Jewel15);
            Jewel16 = CreateSlotVm(ItemSlot.Jewel16);
            Jewel17 = CreateSlotVm(ItemSlot.Jewel17);
            Jewel18 = CreateSlotVm(ItemSlot.Jewel18);
            Jewel19 = CreateSlotVm(ItemSlot.Jewel19);
            Jewel20 = CreateSlotVm(ItemSlot.Jewel20);
            Jewel21 = CreateSlotVm(ItemSlot.Jewel21);
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
            else if(slot == ItemSlot.Jewel || slot == ItemSlot.Jewel02 || slot == ItemSlot.Jewel03 || slot == ItemSlot.Jewel04 || slot == ItemSlot.Jewel05
            || slot == ItemSlot.Jewel06 || slot == ItemSlot.Jewel07 || slot == ItemSlot.Jewel08 || slot == ItemSlot.Jewel09 || slot == ItemSlot.Jewel10
            || slot == ItemSlot.Jewel11 || slot == ItemSlot.Jewel12 || slot == ItemSlot.Jewel13 || slot == ItemSlot.Jewel14 || slot == ItemSlot.Jewel15
            || slot == ItemSlot.Jewel16 || slot == ItemSlot.Jewel17 || slot == ItemSlot.Jewel18 || slot == ItemSlot.Jewel19 || slot == ItemSlot.Jewel20
            || slot == ItemSlot.Jewel21)
            {
                imageName = "Jewel";
            }

            return new InventoryItemViewModel(_dialogCoordinator, _equipmentData, _itemAttributes, slot)
            {
                EmptyBackgroundImagePath = $"/POESKillTree;component/Images/EquipmentUI/ItemDefaults/{imageName}.png"
            };
        }
    }
}