using System.Collections.Generic;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;

namespace POESKillTree.Model.Items
{
    /// <summary>
    /// Interface for <see cref="ItemBase"/> and <see cref="UniqueBase"/>. <see cref="IItemBase.ToString()"/>
    /// is used to get name of the base as it appears in item lists.
    /// </summary>
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
        ItemClass ItemClass { get; }
        Tags Tags { get; }

        /// <summary>
        /// Gets the maximum number of sockets an item of this base can have.
        /// </summary>
        int MaximumNumberOfSockets { get; }

        bool CanHaveQuality { get; }
        IReadOnlyList<IMod> ImplicitMods { get; }

        ItemImage Image { get; }

        /// <param name="quality">If > 0, a quality property will be added.</param>
        /// <returns>The property mods of this item base without influence from implict or explicit mods.
        /// E.g. physical damage, attack speed, armour/evasion/ES, ...</returns>
        List<ItemMod> GetRawProperties(int quality = 0);
    }
}