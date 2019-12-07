using System.Collections.Generic;
using PoESkillTree.Engine.GameModel.Items;

namespace PoESkillTree.Model.Items
{
    // Was previously serialized and deserialized to/from Items.xml. Is now only used as a transfer object between
    // RePoE's base_items JSON and the ItemBase class.
    public class ItemBaseDto
    {
#pragma warning disable CS8618 // These properties are initialized once used

        public IReadOnlyList<string> Implicit { get; set; }

        public IReadOnlyList<string> Properties { get; set; }

        public ItemClass ItemClass { get; set; }

        public Tags Tags { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public int Strength { get; set; }

        public int Dexterity { get; set; }

        public int Intelligence { get; set; }

        public bool DropDisabled { get; set; }

        public int InventoryHeight { get; set; }

        public int InventoryWidth { get; set; }

        public string MetadataId { get; set; }
    }
}