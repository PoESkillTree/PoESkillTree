using System.Collections.Generic;
using System.Linq;
using POESKillTree.Model.Items.Affixes;

namespace POESKillTree.Model.Items
{
    public class UniqueBase
    {

        public string Name { get; }
        public int Level { get; }
        public bool DropDisabled { get; }
        public ItemBase Base { get; }
        public IReadOnlyList<Stat> ExplicitMods { get; }

        public ItemImage Image { get; private set; }

        public UniqueBase(ItemImageService itemImageService, ItemBase itemBase, XmlUnique xmlUnique)
        {
            Name = xmlUnique.Name;
            Level = xmlUnique.Level;
            DropDisabled = xmlUnique.DropDisabled;
            Base = itemBase;
            ExplicitMods = xmlUnique.Explicit.Select(e => new Stat(e, itemBase.ItemType)).ToList();

            Image = itemBase.Image.AsDefaultForUniqueImage(itemImageService, Name);
        }

    }
}