using System.Collections.Generic;

namespace POESKillTree.Model.Items.Mods
{
    public interface IMod
    {
        IReadOnlyList<Stat> Stats { get; }

        ModDomain Domain { get; }

        string Id { get; }

        bool IsEssenceOnly { get; }
    }
}