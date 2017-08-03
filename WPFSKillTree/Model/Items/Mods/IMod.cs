using System.Collections.Generic;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    /// <summary>
    /// Interface for mods as used in <see cref="Affix"/> and crafting view models.
    /// </summary>
    public interface IMod
    {
        IReadOnlyList<IStat> Stats { get; }

        string Name { get; }

        ModDomain Domain { get; }

        int RequiredLevel { get; }
    }
}