using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PoESkillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Enumeration of off hand types a character can have.
    /// </summary>
    public enum OffHand
    {
        [Description("Dual wield")]
        DualWield,
        [Description("Shield")]
        Shield,
        [Description("Two handed")]
        TwoHanded
    }

    public static class OffHandExtensions
    {
        private static readonly Dictionary<OffHand, string[]> Aliases = new Dictionary<OffHand, string[]>()
        {
            {OffHand.DualWield, new [] {"dual wield", "dualwield"} },
            {OffHand.Shield, new [] {"shield"} },
            {OffHand.TwoHanded, new [] {"two handed", "twohanded"} }
        };

        /// <summary>
        /// Returns whether the given OffHand has the given string as an alias.
        /// (case insensitive)
        /// </summary>
        public static bool HasAlias(this OffHand offHand, string alias)
        {
            if (alias == null) return false;
            alias = alias.ToLowerInvariant();
            return Aliases[offHand].Any(s => s == alias);
        }
    }
}