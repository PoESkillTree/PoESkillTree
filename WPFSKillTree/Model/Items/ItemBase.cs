using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;

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
        public ItemClass ItemClass { get; }
        public Tags Tags { get; }
        public string MetadataId { get; }

        public int MaximumNumberOfSockets { get; }

        public bool CanHaveQuality { get; }
        public IReadOnlyList<IMod> ImplicitMods { get; }
        private readonly IReadOnlyList<string> _properties;

        public ItemImage Image { get; }

        public ItemBase(ItemImageService itemImageService, ModDatabase modDatabase, XmlItemBase xmlBase)
        {
            Level = xmlBase.Level;
            RequiredStrength = xmlBase.Strength;
            RequiredDexterity = xmlBase.Dexterity;
            RequiredIntelligence = xmlBase.Intelligence;
            DropDisabled = xmlBase.DropDisabled;
            InventoryHeight = xmlBase.InventoryHeight;
            InventoryWidth = xmlBase.InventoryWidth;

            Name = xmlBase.Name;
            ItemClass = xmlBase.ItemClass;
            Tags = xmlBase.Tags;
            MetadataId = xmlBase.MetadataId;

            ImplicitMods = xmlBase.Implicit.Select(id => modDatabase.Mods[id]).ToList();
            _properties = xmlBase.Properties;
            CanHaveQuality = Tags.HasFlag(Tags.Weapon) || Tags.HasFlag(Tags.Armour);

            Image = new ItemImage(itemImageService, Name, ItemClass);

            MaximumNumberOfSockets = GetMaximumNumberOfSockets();
        }

        /// <summary>
        /// Creates an ItemBase that sets <see cref="ItemClass"/> and <see cref="Tags"/> on
        /// a best effort basis. They might not be set correctly.
        /// <para/>
        /// Only <see cref="Name"/>, <see cref="ItemClass"/> and <see cref="Tags"/> may be called on
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
            ImplicitMods = new List<IMod>();
            _properties = new List<string>();
            CanHaveQuality = false;

            Name = typeLine;
            ItemClass = ItemSlotToClass(itemSlot);
            if (ItemClass == ItemClass.ActiveSkillGem)
            {
                ItemClass = ItemClassEx.ItemClassForGem(typeLine);
            }
            if (ItemClass == ItemClass.Unknown)
            {
                if (frameType == FrameType.Gem)
                {
                    ItemClass = ItemClassEx.ItemClassForGem(typeLine);
                }
                else if (frameType == FrameType.Currency || frameType == FrameType.DivinationCard
                         || frameType == FrameType.QuestItem || frameType == FrameType.Prophecy)
                {
                    ItemClass = ItemClass.Unknown;
                }
                else if (typeLine.Contains("Quiver"))
                {
                    ItemClass = ItemClass.Quiver;
                }
                else if (typeLine.Contains("Shield") || typeLine.Contains("Buckler"))
                {
                    ItemClass = ItemClass.Shield;
                }
                else if (typeLine.Contains("Amulet") || typeLine.Contains("Talisman"))
                {
                    ItemClass = ItemClass.Amulet;
                }
                else if (typeLine.Contains("Ring"))
                {
                    ItemClass = ItemClass.Ring;
                }
                else if (typeLine.Contains("Belt"))
                {
                    ItemClass = ItemClass.Belt;
                }
                else if (!string.IsNullOrWhiteSpace(weaponClass))
                {
                    // This will not catch ThrustingOneHandSword and Sceptre,
                    // but the distinction between those and OneHandSword and OneHandMace only matters for mod crafting
                    var itemClassStr = weaponClass.Replace("Handed", "Hand")
                        .Replace(" ", "").Trim();
                    ItemClass type;
                    if (Enum.TryParse(itemClassStr, true, out type))
                    {
                        ItemClass = type;
                    }
                }
            }

            // This might miss some tags, but those are only important for mod crafting, 
            // which will not happen with this item.
            Tags = ItemClass.ToTags();

            Image = new ItemImage(itemImageService, ItemClass);
        }

        /// <summary>
        /// Returns the <see cref="ItemClass"/> that fits <paramref name="itemSlot"/>
        /// if it's not ambigous.
        /// </summary>
        private static ItemClass ItemSlotToClass(ItemSlot itemSlot)
        {
            switch (itemSlot)
            {
                case ItemSlot.BodyArmour:
                    return ItemClass.BodyArmour;
                case ItemSlot.Ring:
                case ItemSlot.Ring2:
                    return ItemClass.Ring;
                case ItemSlot.Amulet:
                    return ItemClass.Amulet;
                case ItemSlot.Helm:
                    return ItemClass.Helmet;
                case ItemSlot.Gloves:
                    return ItemClass.Gloves;
                case ItemSlot.Boots:
                    return ItemClass.Boots;
                case ItemSlot.Gem:
                    return ItemClass.ActiveSkillGem;
                case ItemSlot.Belt:
                    return ItemClass.Belt;
                default: // MainHand, OffHand, Unequippable
                    return ItemClass.Unknown;
            }
        }

        public List<ItemMod> GetRawProperties(int quality = 0)
        {
            var props = new List<ItemMod>();

            if (Tags.HasFlag(Tags.Weapon))
            {
                var itemClass = ItemClass;
                if (itemClass == ItemClass.Sceptre)
                    itemClass = ItemClass.OneHandMace;
                else if (itemClass == ItemClass.ThrustingOneHandSword)
                    itemClass = ItemClass.OneHandSword;
                // replace "Hand" by "Handed" and add a space in front of each capital letter
                var itemClassStr = Regex.Replace(itemClass.ToString().Replace("Hand", "Handed"), 
                    @"([a-z])([A-Z])", @"$1 $2");
                props.Add(new ItemMod(itemClassStr, true));
            }

            if (quality > 0)
            {
                var qProp = new ItemMod($"Quality: +{quality}%", true, ItemMod.ValueColoring.LocallyAffected);
                props.Add(qProp);
            }

            if (_properties != null)
                props.AddRange(_properties.Select(prop => new ItemMod(prop, true)));
            return props;
        }

        public override string ToString()
        {
            return Name;
        }

        private int GetMaximumNumberOfSockets()
        {
            var socketStat = ImplicitMods
                .SelectMany(m => m.Stats)
                .FirstOrDefault(s => s.Id == "local_has_X_sockets");
            if (socketStat != null)
            {
                // e.g. Unset Ring
                return socketStat.Range.From;
            }
            if (Tags.HasFlag(Tags.OneHand) || Tags.HasFlag(Tags.Shield))
            {
                return 3;
            }
            if (Tags.HasFlag(Tags.FishingRod))
            {
                return 4;
            }
            if (Tags.HasFlag(Tags.TwoHand) || Tags.HasFlag(Tags.BodyArmour))
            {
                return 6;
            }
            if (Tags.HasFlag(Tags.Armour))
            {
                // exceptions (Shield and BodyArmour) are handled above
                return 4;
            }
            return 0;
        }
    }
}
