using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items
{
    /// <summary>
    /// Represents an item base, e.g. Harbinger Bow or Glorious Plate.
    /// </summary>
    public class ItemBase : IItemBase
    {

        public int Level { get; }
        public int RequiredStrength { get; }
        public int RequiredDexterity { get; }
        public int RequiredIntelligence { get; }
        public bool DropDisabled { get; }
        public int InventoryHeight { get; }
        public int InventoryWidth { get; }

        public string Name { get; }
        public ItemType ItemType { get; }
        public ItemGroup ItemGroup { get; }
        public string MetadataId { get; }

        public bool CanHaveQuality { get; }
        public IReadOnlyList<Stat> ImplicitMods { get; }
        private readonly IReadOnlyList<Stat> _properties;

        public ItemImage Image { get; }

        public ItemBase(ItemImageService itemImageService, XmlItemBase xmlBase)
        {
            Level = xmlBase.Level;
            RequiredStrength = xmlBase.Strength;
            RequiredDexterity = xmlBase.Dexterity;
            RequiredIntelligence = xmlBase.Intelligence;
            DropDisabled = xmlBase.DropDisabled;
            InventoryHeight = xmlBase.InventoryHeight;
            InventoryWidth = xmlBase.InventoryWidth;

            Name = xmlBase.Name;
            ItemType = xmlBase.ItemType;
            ItemGroup = ItemType.Group();
            MetadataId = xmlBase.MetadataId;

            ImplicitMods = xmlBase.Implicit.Select(i => new Stat(i, ItemType)).ToList();
            _properties = xmlBase.Properties.Select(p => new Stat(p, ItemType)).ToList();
            CanHaveQuality = ItemGroup == ItemGroup.OneHandedWeapon || ItemGroup == ItemGroup.TwoHandedWeapon
                             || ItemGroup == ItemGroup.BodyArmour || ItemGroup == ItemGroup.Boots
                             || ItemGroup == ItemGroup.Gloves || ItemGroup == ItemGroup.Helmet
                             || ItemGroup == ItemGroup.Shield;

            Image = new ItemImage(itemImageService, Name, ItemGroup);
        }

        /// <summary>
        /// Creates an ItemBase that sets <see cref="ItemGroup"/> and <see cref="ItemType"/> on
        /// a best effort basis. They might not be set correctly.
        /// <para/>
        /// Only <see cref="Name"/>, <see cref="ItemGroup"/> and <see cref="ItemType"/> may be called on
        /// ItemBases created via this constructor. It is not meant to produce bases that can exist independent
        /// of the <see cref="Item"/> they are created for.
        /// </summary>
        /// <param name="itemImageService"></param>
        /// <param name="itemSlot">The slot the parent <see cref="Item"/> is slotted into.
        /// <see cref="ItemSlot.Unequipable"/> if is not equipped.</param>
        /// <param name="typeLine">The TypeLine property of the parent <see cref="Item"/>.</param>
        /// <param name="weaponClass">A string representing the weapon class of the parent <see cref="Item"/>.
        /// Can be null or empty if that item is not a weapon. The weapon class generally is a property without value.</param>
        /// <param name="frameType">The frame type of the item.</param>
        public ItemBase(ItemImageService itemImageService,
            ItemSlot itemSlot, string typeLine, string weaponClass, FrameType frameType)
        {
            // These don't matter as we won't create new items from this base.
            Level = 0;
            RequiredStrength = 0;
            RequiredDexterity = 0;
            RequiredIntelligence = 0;
            DropDisabled = false;
            InventoryHeight = 0;
            InventoryWidth = 0;
            MetadataId = "";
            ImplicitMods = new List<Stat>();
            _properties = new List<Stat>();
            CanHaveQuality = false;

            Name = typeLine;
            ItemGroup = ItemSlotToGroup(itemSlot);
            if (ItemGroup != ItemGroup.Unknown)
            {
                // This might be wrong for Armour slots, but the item will most likely not be edited so this is not important.
                ItemType = ItemGroup.Types()[0];
            }
            else if (frameType == FrameType.Gem)
                ItemType = ItemType.Gem;
            else if (frameType == FrameType.Currency || frameType == FrameType.DivinationCard
                || frameType == FrameType.QuestItem || frameType == FrameType.Prophecy)
                ItemType = ItemType.Unknown;
            else if (typeLine.Contains("Quiver"))
                ItemType = ItemType.Quiver;
            else if (typeLine.Contains("Shield") || typeLine.Contains("Buckler"))
                ItemType = ItemType.ShieldArmour;
            else if (typeLine.Contains("Amulet") || typeLine.Contains("Talisman"))
                ItemType = ItemType.Amulet;
            else if (typeLine.Contains("Ring"))
                ItemType = ItemType.Ring;
            else if (typeLine.Contains("Belt"))
                ItemType = ItemType.Belt;
            else if (!string.IsNullOrEmpty(weaponClass.Trim()))
            {
                ItemType type;
                if (Enum.TryParse(Regex.Replace(weaponClass.Trim(), "([a-z]) ([A-Z])", "$1$2"), true, out type))
                    ItemType = type;
            }

            if (ItemGroup == ItemGroup.Unknown)
                ItemGroup = ItemType.Group();
            Image = new ItemImage(itemImageService, ItemGroup);
        }

        /// <summary>
        /// Returns the <see cref="ItemGroup"/> that fits <paramref name="itemSlot"/>
        /// if it's not ambigous.
        /// </summary>
        private static ItemGroup ItemSlotToGroup(ItemSlot itemSlot)
        {
            switch (itemSlot)
            {
                case ItemSlot.Armor:
                    return ItemGroup.BodyArmour;
                case ItemSlot.Ring:
                case ItemSlot.Ring2:
                    return ItemGroup.Ring;
                case ItemSlot.Amulet:
                    return ItemGroup.Amulet;
                case ItemSlot.Helm:
                    return ItemGroup.Helmet;
                case ItemSlot.Gloves:
                    return ItemGroup.Gloves;
                case ItemSlot.Boots:
                    return ItemGroup.Boots;
                case ItemSlot.Gem:
                    return ItemGroup.Gem;
                case ItemSlot.Belt:
                    return ItemGroup.Belt;
                default: // MainHand, OffHand, Unequippable
                    return ItemGroup.Unknown;
            }
        }

        public List<ItemMod> GetRawProperties(int quality = 0)
        {
            var props = new List<ItemMod>();

            if (ItemGroup == ItemGroup.TwoHandedWeapon || ItemGroup == ItemGroup.OneHandedWeapon)
            {
                var type = ItemType;
                if (type == ItemType.Sceptre)
                    type = ItemType.OneHandedMace;
                else if (type == ItemType.ThrustingOneHandedSword)
                    type = ItemType.OneHandedSword;
                props.Add(new ItemMod(ItemType, Regex.Replace(type.ToString(), @"([a-z])([A-Z])", @"$1 $2")));
            }

            if (quality > 0)
            {
                var qProp = new ItemMod(ItemType, "Quality: +#%");
                qProp.Value.Add(quality);
                qProp.ValueColor.Add(ItemMod.ValueColoring.LocallyAffected);
                props.Add(qProp);
            }

            if (_properties != null)
                props.AddRange(_properties.Select(prop => prop.AsPropertyToItemMod()));
            return props;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
