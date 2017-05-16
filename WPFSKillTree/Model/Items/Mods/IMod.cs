using System.Collections.Generic;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    public interface IMod
    {
        IReadOnlyList<IStat> Stats { get; }

        string Name { get; }

        ModDomain Domain { get; }

        int RequiredLevel { get; }
    }
}