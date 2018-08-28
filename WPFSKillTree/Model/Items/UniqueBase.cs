using System.Collections.Generic;
using System.Linq;
using log4net;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;

namespace POESKillTree.Model.Items
{
    /// <summary>
    /// Represents the base of an unique item. I.e. their base item and ranges of explicit mods.
    /// </summary>
    public class UniqueBase : IItemBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UniqueBase));

        public int Level { get; }
        public int RequiredStrength => _base.RequiredStrength;
        public int RequiredDexterity => _base.RequiredDexterity;
        public int RequiredIntelligence => _base.RequiredIntelligence;
        public bool DropDisabled { get; }
        public int InventoryHeight => _base.InventoryHeight;
        public int InventoryWidth => _base.InventoryWidth;

        public string UniqueName { get; }
        public string Name => _base.Name;
        public ItemClass ItemClass => _base.ItemClass;
        public Tags Tags => _base.Tags;
        public int MaximumNumberOfSockets => _base.MaximumNumberOfSockets;

        private readonly ItemBase _base;
        public bool CanHaveQuality => _base.CanHaveQuality;
        private readonly IReadOnlyList<string> _properties;
        public IReadOnlyList<IMod> ImplicitMods => _base.ImplicitMods;
        public IReadOnlyList<IMod> ExplicitMods { get; }

        public ItemImage Image { get; }

        public UniqueBase(ItemImageService itemImageService, ModDatabase modDatabase, ItemBase itemBase, 
            XmlUnique xmlUnique)
        {
            UniqueName = xmlUnique.Name;
            Level = xmlUnique.Level;
            DropDisabled = xmlUnique.DropDisabled;
            _base = itemBase;
            _properties = xmlUnique.Properties;
            var explicits = new List<IMod>();
            foreach (var id in xmlUnique.Explicit)
            {
                Mod mod;
                if (!modDatabase.Mods.TryGetValue(id, out mod))
                {
                    Log.Error($"Unknown mod id {id} on unique {UniqueName}");
                    continue;
                }
                explicits.Add(mod);
            }
            ExplicitMods = explicits;

            Image = itemBase.Image.AsDefaultForUniqueImage(itemImageService, UniqueName);
        }

        public override string ToString()
        {
            return UniqueName;
        }

        public List<ItemMod> GetRawProperties(int quality = 0)
        {
            var mods = _base.GetRawProperties(quality);
            mods.AddRange(_properties.Select(prop => new ItemMod(prop, true)));
            return mods;
        }
    }
}