using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public enum OffHand
    {
        DualWield, Shield, TwoHanded
    }

    public static class OffHandExtensions
    {
        private static readonly Dictionary<OffHand, string[]> Aliases = new Dictionary<OffHand, string[]>()
        {
            {OffHand.DualWield, new [] {"Dual Wield", "DualWield"} },
            {OffHand.Shield, new [] {"Shield"} },
            {OffHand.TwoHanded, new [] {"Two handed", "TwoHanded"} }
        };

        public static bool HasAlias(this OffHand offHand, string alias)
        {
            alias = alias.ToLowerInvariant();
            return Aliases.Any(pair => pair.Key == offHand && pair.Value.Any(s => s == alias));
        }
    }
}