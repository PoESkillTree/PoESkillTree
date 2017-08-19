using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using MoreLinq;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
#if (PoESkillTree_UseSmallDec_ForAttributes)
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#endif
    using SmallDigit =
#if (PoESkillTree_UseSmallDec_ForAttributes)
    SmallDec;
#else
    System.Single;
#endif
    /// <summary>
    /// Enumeration of the bandits from whom can be choose at each difficulty.
    /// (choosing none gives 2 extra skill points)
    /// </summary>
    public enum Bandit
    {
        None = 0,
        Alira = 1,
        Oak = 2,
        Kraityn = 3
    }

    /// <summary>
    /// Stores the bandit choice.
    /// </summary>
    public class BanditSettings : Notifier
    {
        private static readonly Dictionary<Bandit, IReadOnlyList<(string stat, SmallDigit value)>>
            RewardsPerBandit = new Dictionary<Bandit, IReadOnlyList<(string stat, SmallDigit value)>>
            {
                { Bandit.None, new (string stat, SmallDigit value)[0] },
                {
                    Bandit.Alira,
                    new[]
                    {
#if (!PoESkillTree_UseSmallDec_ForAttributes)
                        ("+# Mana Regenerated per second", 5F),
                        ("+#% to Global Critical Strike Multiplier", 20F),
                        ("+#% to all Elemental Resistances", 15F)
#else
                        ("+# Mana Regenerated per second", 5),
                        ("+#% to Global Critical Strike Multiplier", 20),
                        ("+#% to all Elemental Resistances", 15)
#endif
                    }
                },
                {
                    Bandit.Oak,
                    new []
                    {
#if (!PoESkillTree_UseSmallDec_ForAttributes)
                        ("#% of Life Regenerated per second", 1F),
                        ("#% additional Physical Damage Reduction", 2F),
                        ("#% increased Physical Damage", 20F)
#else
                        ("#% of Life Regenerated per second", 1),
                        ("#% additional Physical Damage Reduction", 2),
                        ("#% increased Physical Damage", 20)
#endif
                    }
                },
                {
                    Bandit.Kraityn,
                    new []
                    {
#if (!PoESkillTree_UseSmallDec_ForAttributes)
                        ("#% increased Attack and Cast Speed", 6F),
                        ("#% chance to Dodge Attacks", 3F),
                        ("#% increased Movement Speed", 6F)
#else
                        ("#% increased Attack and Cast Speed", 6),
                        ("#% chance to Dodge Attacks", 3),
                        ("#% increased Movement Speed", 6)
#endif
                    }
                }
            };

        private Bandit _choice;
        
        public Bandit Choice
        {
            get { return _choice; }
            set { SetProperty(ref _choice, value); }
        }

        [XmlIgnore]
        public Dictionary<string, SmallDigit> Rewards => RewardsPerBandit[Choice].ToDictionary();

        public static IEnumerable<Bandit> BanditValues
        {
            get { return Enum.GetValues(typeof(Bandit)).Cast<Bandit>(); }
        }

        public void Reset()
        {
            Choice = Bandit.None;
        }

        /// <summary>
        /// Returns a deep copy of this instance. (event handlers are NOT cloned)
        /// </summary>
        public BanditSettings DeepClone()
        {
            return (BanditSettings) SafeMemberwiseClone();
        }
    }
}