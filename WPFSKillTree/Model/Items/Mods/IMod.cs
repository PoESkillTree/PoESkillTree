using System.Collections.Generic;
using PoESkillTree.GameModel.Modifiers;

namespace POESKillTree.Model.Items.Mods
{
    /// <summary>
    /// Interface for mods as used in <see cref="Affix"/> and crafting view models.
    /// </summary>
    public interface IMod
    {
        IReadOnlyList<Stat> Stats { get; }

        string Name { get; }

        ModDomain Domain { get; }

        int RequiredLevel { get; }
    }
}