using System.Collections.Generic;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items
{
    public interface IItemBase
    {
        int Level { get; }
        int RequiredStrength { get; }
        int RequiredDexterity { get; }
        int RequiredIntelligence { get; }
        bool DropDisabled { get; }
        int InventoryHeight { get; }
        int InventoryWidth { get; }

        string Name { get; }
        ItemType ItemType { get; }
        ItemGroup ItemGroup { get; }

        bool CanHaveQuality { get; }
        IReadOnlyList<Stat> ImplicitMods { get; }

        ItemImage Image { get; }

        List<ItemMod> GetRawProperties(int quality = 0);
    }
}