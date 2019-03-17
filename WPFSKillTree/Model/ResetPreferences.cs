using System;

namespace PoESkillTree.Model
{
    /// <summary>
    /// Specifies which build data is reset on reset button click.
    /// </summary>
    [Flags]
    public enum ResetPreferences
    {
        MainTree = 1,
        AscendancyTree = 2,
        Bandits = 4
    }
}