using System.Collections.Generic;

namespace POESKillTree.Model.Items.Mods
{
    public interface IMod
    {
        IReadOnlyList<IStat> Stats { get; }

        string Id { get; }

        int RequiredLevel { get; }
    }
}