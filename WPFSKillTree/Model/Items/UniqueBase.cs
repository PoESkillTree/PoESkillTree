using System.Collections.Generic;
using System.Linq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items
{
    public class UniqueBase : IItemBase
    {

        public int Level { get; }
        public int RequiredStrength => _base.RequiredStrength;
        public int RequiredDexterity => _base.RequiredDexterity;
        public int RequiredIntelligence => _base.RequiredIntelligence;
        public bool DropDisabled { get; }
        public int InventoryHeight => _base.InventoryHeight;
        public int InventoryWidth => _base.InventoryWidth;

        public string UniqueName { get; }
        public string Name => _base.Name;
        public ItemType ItemType => _base.ItemType;
        public ItemGroup ItemGroup => _base.ItemGroup;

        private readonly ItemBase _base;
        public bool CanHaveQuality => _base.CanHaveQuality;
        public IReadOnlyList<Stat> ImplicitMods => _base.ImplicitMods;
        public IReadOnlyList<Stat> ExplicitMods { get; }

        public ItemImage Image { get; }

        public UniqueBase(ItemImageService itemImageService, ItemBase itemBase, XmlUnique xmlUnique)
        {
            UniqueName = xmlUnique.Name;
            Level = xmlUnique.Level;
            DropDisabled = xmlUnique.DropDisabled;
            _base = itemBase;
            ExplicitMods = xmlUnique.Explicit.Select(e => new Stat(e, itemBase.ItemType)).ToList();

            Image = itemBase.Image.AsDefaultForUniqueImage(itemImageService, UniqueName);
        }

        public override string ToString()
        {
            return UniqueName;
        }

        public List<ItemMod> GetRawProperties(int quality = 0)
        {
            return _base.GetRawProperties(quality);
        }
    }
}