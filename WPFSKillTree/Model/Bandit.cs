using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using MoreLinq;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    /// <summary>
    /// Enumeration of the bandits from whom can be choose at each difficulty.
    /// (choosing none gives an extra skill point)
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
        private static readonly Dictionary<Bandit, IReadOnlyList<(string stat, float value)>>
            RewardsPerBandit = new Dictionary<Bandit, IReadOnlyList<(string stat, float value)>>
            {
                { Bandit.None, new (string stat, float value)[0] },
                {
                    Bandit.Alira,
                    new[]
                    {
                        ("+# Mana Regenerated per second", 5F),
                        ("+#% to Global Critical Strike Multiplier", 20F),
                        ("+#% to all Elemental Resistances", 15F)
                    }
                },
                {
                    Bandit.Oak,
                    new []
                    {
                        ("#% of Life Regenerated per second", 1F),
                        ("#% additional Physical Damage Reduction", 2F),
                        ("#% increased Physical Damage", 20F)
                    }
                },
                {
                    Bandit.Kraityn,
                    new []
                    {
                        ("#% increased Attack and Cast Speed", 6F),
                        ("#% chance to Dodge Attacks", 3F),
                        ("#% increased Movement Speed", 6F)
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
        public Dictionary<string, float> Rewards => RewardsPerBandit[Choice].ToDictionary();

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