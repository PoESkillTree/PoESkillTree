using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{

    public class ItemBase
    {
        public static readonly IReadOnlyList<ItemBase> BaseList;

        public static readonly IReadOnlyDictionary<string, ItemBase> BaseDictionary;

        private static readonly WordSetTreeNode Root;

        static ItemBase()
        {
            var filename = Path.Combine(AppData.GetFolder(@"Data\Equipment"), @"ItemList.xml");
            if (File.Exists(filename))
            {
                using (var reader = new StreamReader(filename))
                {
                    var ser = new XmlSerializer(typeof(XmlItemList));
                    var xmlList = (XmlItemList) ser.Deserialize(reader);
                    BaseList = xmlList.ItemBases.Select(x => new ItemBase(x)).ToList();
                }
            }
            else
            {
                BaseList = new List<ItemBase>();
            }

            // todo The other two-stone rings are not selected by this. Needs disambiguation.
            var dict = new Dictionary<string, ItemBase>();
            foreach (var itemBase in BaseList)
            {
                dict[itemBase.Name] = itemBase;
            }
            BaseDictionary = dict;

            Root = new WordSetTreeNode();
            var allbs = BaseList.Select(m => m.Name.Split(' ')).OrderBy(s => s[0]).ToArray();
            foreach (var wl in allbs)
                Root.AddWordset(wl);
        }

        public static ItemBase ItemBaseFromTypeline(string typeline)
        {
            var wlist = typeline.Split(' ');
            var ms = new List<WordSetTreeNode>();

            for (int i = 0; i < wlist.Length; i++)
            {
                var m = Root.Match(wlist.Skip(i)).OrderByDescending(n => n.Level).FirstOrDefault();
                if (m != null)
                    ms.Add(m);
            }

            if (ms.Count == 0)
                return null;
            if (ms.Count > 1)
                throw new NotSupportedException("duplicate type match");
            return BaseDictionary[ms[0].ToString()];
        }

        private static int GetWidthForItem(ItemType type, ItemGroup group, string name)
        {
            switch (group)
            {
                case ItemGroup.Helmet:
                case ItemGroup.BodyArmour:
                case ItemGroup.Belt:
                case ItemGroup.Gloves:
                case ItemGroup.Boots:
                case ItemGroup.Quiver:
                case ItemGroup.Shield:
                case ItemGroup.TwoHandedWeapon:
                    return name == "Corroded Blade" ? 1 : 2;
            }
            switch (type)
            {
                case ItemType.OneHandedAxe:
                case ItemType.Claw:
                case ItemType.Sceptre:
                    return 2;

                case ItemType.OneHandedMace:
                    if (name.EndsWith("Club") || name == "Tenderizer")
                        return 1;
                    return 2;

                case ItemType.OneHandedSword:
                    switch (name)
                    {
                        case "Rusted Sword":
                        case "Gemstone Sword":
                        case "Corsair Sword":
                        case "Cutlass":
                        case "Variscite Blade":
                        case "Sabre":
                        case "Copper Sword":
                            return 1;
                    }
                    return 2;

            }

            // Thrusting swords, rings, amulets
            return 1;
        }

        private static int GetHeightForItem(ItemType type, ItemGroup group, string name)
        {
            switch (group)
            {
                case ItemGroup.TwoHandedWeapon:
                    return name.EndsWith("Crude Bow") || name.EndsWith("Short Bow") || name.EndsWith("Grove Bow") || name.EndsWith("Thicket Bow") ? 3 : 4;
                case ItemGroup.Helmet:
                case ItemGroup.Gloves:
                case ItemGroup.Boots:
                    return 2;

                case ItemGroup.Shield:
                    if (name.EndsWith("Kite Shield") || name.EndsWith("Round Shield"))
                        return 3;
                    if (name.EndsWith("Tower Shield"))
                        return 4;
                    return 2;

                case ItemGroup.Quiver:
                case ItemGroup.BodyArmour:
                    return 3;
            }

            // belts, amulets, rings
            if (group != ItemGroup.OneHandedWeapon) return 1;

            switch (type)
            {
                case ItemType.Claw:
                    return 2;
                case ItemType.Dagger:
                case ItemType.Wand:
                case ItemType.OneHandedAxe:
                case ItemType.OneHandedMace:
                case ItemType.Sceptre:
                case ItemType.OneHandedSword:
                    return 3;
                case ItemType.ThrustingOneHandedSword:
                    return 4;
                default:
                    return 1;
            }
        }

        public int Level { get; private set; }
        public int RequiredStrength { get; private set; }
        public int RequiredDexterity { get; private set; }
        public int RequiredIntelligence { get; private set; }

        public string Name { get; private set; }
        public ItemType ItemType { get; private set; }
        public ItemGroup ItemGroup { get; private set; }

        public IReadOnlyList<Stat> ImplicitMods { get; private set; }
        private IReadOnlyList<Stat> Properties { get; set; }

        private ItemBase(XmlItemBase xmlBase)
        {
            Level = xmlBase.Level;
            RequiredStrength = xmlBase.Strength;
            RequiredDexterity = xmlBase.Dexterity;
            RequiredIntelligence = xmlBase.Intelligence;

            Name = xmlBase.Name;
            ItemType = xmlBase.ItemType;
            ItemGroup = ItemType.Group();
            ImplicitMods = xmlBase.Implicit != null
                ? xmlBase.Implicit.Select(i => new Stat(i, ItemType)).ToList()
                : new List<Stat>();
            Properties = xmlBase.Properties != null
                ? xmlBase.Properties.Select(p => new Stat(p, ItemType)).ToList()
                : new List<Stat>();
        }

        /// <summary>
        /// Creates an ItemBase that sets <see cref="ItemGroup"/> and <see cref="ItemType"/> on
        /// a best effort basis. They might not be set correctly.
        /// <para/>
        /// Only <see cref="Name"/>, <see cref="ItemGroup"/> and <see cref="ItemType"/> may be called on
        /// ItemBases created via this constructor. It is not meant to produce bases that can exist independent
        /// of the <see cref="Item"/> they are created for.
        /// </summary>
        /// <param name="itemSlot">The slot the parent <see cref="Item"/> is slotted into.
        /// <see cref="ItemSlot.Unequipable"/> if is not equipped.</param>
        /// <param name="typeLine">The TypeLine property of the parent <see cref="Item"/>.</param>
        /// <param name="weaponClass">A string representing the weapon class of the parent <see cref="Item"/>.
        /// Can be null or empty if that item is not a weapon. The weapon class generally is a property without value.</param>
        public ItemBase(ItemSlot itemSlot, string typeLine, string weaponClass)
        {
            // These don't matter as we won't create new items from this base.
            Level = 0;
            RequiredStrength = 0;
            RequiredDexterity = 0;
            RequiredIntelligence = 0;
            ImplicitMods = new List<Stat>();
            Properties = new List<Stat>();

            Name = typeLine;
            ItemGroup = ItemSlotToGroup(itemSlot);
            if (ItemGroup != ItemGroup.Unknown)
            {
                // This might be wrong for Armour slots, but the item will most likely not be edited so this is not important.
                ItemType = ItemGroup.Types()[0];
                return;
            }
            if (typeLine.Contains("Quiver"))
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
            ItemGroup = ItemType.Group();
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

        public Item CreateItem()
        {
            return new Item(this, GetWidthForItem(ItemType, ItemGroup, Name), GetHeightForItem(ItemType, ItemGroup, Name))
            {
                Properties = GetRawProperties()
            };
        }

        public List<ItemMod> GetRawProperties()
        {
            var props = new List<ItemMod>();

            if (ItemGroup == ItemGroup.TwoHandedWeapon || ItemGroup == ItemGroup.OneHandedWeapon)
                props.Add(new ItemMod(ItemType, Regex.Replace(ItemType.ToString(), @"([a-z])([A-Z])", @"$1 $2")));

            if (Properties != null)
                props.AddRange(Properties.Select(prop => prop.ToItemMod()));
            return props;
        }

        public override string ToString()
        {
            return Name;
        }

        private class WordSetTreeNode
        {
            public int Level { get; private set; }

            private string _word;

            private WordSetTreeNode _parent;

            private bool _isLeaf;

            private readonly Dictionary<string, WordSetTreeNode> _children = new Dictionary<string, WordSetTreeNode>();

            public void AddWordset(IEnumerable<string> words)
            {
                var enumerated = words as string[] ?? words.ToArray();

                var word = enumerated.First();

                WordSetTreeNode nod;
                if (!_children.TryGetValue(word, out nod))
                {
                    nod = new WordSetTreeNode {_word = word, _parent = this, Level = Level + 1};
                    _children.Add(word, nod);
                }

                if (enumerated.Length > 1)
                    nod.AddWordset(enumerated.Skip(1));
                else
                    nod._isLeaf = true;
            }

            public IEnumerable<WordSetTreeNode> Match(IEnumerable<string> enumerable)
            {
                var nod = this;
                foreach (var w in enumerable)
                {
                    if (nod._isLeaf)
                        yield return nod;

                    if (nod._children == null || !nod._children.TryGetValue(w, out nod))
                        yield break;
                }

                if (nod._isLeaf)
                    yield return nod;
            }

            public override string ToString()
            {
                var lst = new Stack<string>();
                var nod = this;
                while (nod._parent != null)
                {
                    lst.Push(nod._word);
                    nod = nod._parent;
                }

                return string.Join(" ", lst);
            }
        }
    }
}
