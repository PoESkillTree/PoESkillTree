using System.Collections.Generic;
using System.ComponentModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Model.Items.Mods;

namespace PoESkillTree.Model.Items
{
    public interface IHasItemToolTip : INotifyPropertyChanged
    {
        FrameType Frame { get; }

        string NameLine { get; }

        bool HasNameLine { get; }

        string TypeLine { get; }

        IReadOnlyList<ItemMod> Properties { get; }

        IReadOnlyList<ItemMod> Requirements { get; }

        IReadOnlyList<ItemMod> ImplicitMods { get; }

        IReadOnlyList<ItemMod> ExplicitMods { get; }

        IReadOnlyList<ItemMod> CraftedMods { get; }

        string? FlavourText { get; }

        bool HasFlavourText { get; }
    }
}