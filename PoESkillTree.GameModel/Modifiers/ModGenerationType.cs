namespace PoESkillTree.GameModel.Modifiers
{
    /// <summary>
    /// The generation type of a mod as it appears in the GGPK.
    /// See
    /// http://omegak2.net/poe/PyPoE/_autosummary/PyPoE.poe.constants.html#PyPoE.poe.constants.MOD_GENERATION_TYPE
    /// for more information.
    /// </summary>
    public enum ModGenerationType
    {
        Prefix,
        Suffix,
        Unique,
        Corrupted,
        Tempest,
        Enchantment,
    }
}