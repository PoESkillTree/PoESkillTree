using System.Runtime.Serialization;

namespace PoESkillTree.GameModel.Modifiers
{
    /// <summary>
    /// The domain of a mod as it appears in the GGPK.
    /// See
    /// http://omegak2.net/poe/PyPoE/_autosummary/PyPoE.poe.constants.html#PyPoE.poe.constants.MOD_DOMAIN
    /// for more information.
    /// </summary>
    public enum ModDomain
    {
        Item,
        Flask,
        Crafted,
        Area,
        Misc,
        [EnumMember(Value = "abyss_jewel")]
        AbyssJewel,
        Atlas,
        Delve,
    }
}