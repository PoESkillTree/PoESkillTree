using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
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
    /// Enumeration of the in-game difficulties.
    /// </summary>
    public enum Difficulty
    {
        Normal,
        Cruel,
        Merciless
    }

    /// <summary>
    /// Extension class for <see cref="Bandit"/> that stores the reward attributes for each bandit and difficulty.
    /// </summary>
    public static class BanditExtensions
    {
#if (PoESkillTree_UseSmallDec_ForAttributes)
        private static readonly Dictionary<Tuple<Bandit, Difficulty>, Tuple<string, SmallDec>> Rewards = new Dictionary<Tuple<Bandit, Difficulty>, Tuple<string, SmallDec>>
#else
        private static readonly Dictionary<Tuple<Bandit, Difficulty>, Tuple<string, float>> Rewards = new Dictionary<Tuple<Bandit, Difficulty>, Tuple<string, float>>
#endif
        {
            {Tuple.Create(Bandit.Alira, Difficulty.Normal), Tuple.Create("+# to maximum Mana",
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                60F)},
            {Tuple.Create(Bandit.Alira, Difficulty.Cruel), Tuple.Create("#% increased Cast Speed", 
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                5F)},
            {Tuple.Create(Bandit.Alira, Difficulty.Merciless), Tuple.Create("+# to Maximum Power Charges",
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                1F)},
            {Tuple.Create(Bandit.Oak, Difficulty.Normal), Tuple.Create("+# to maximum Life", 
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                40F)},
            {Tuple.Create(Bandit.Oak, Difficulty.Cruel), Tuple.Create("#% increased Physical Damage", 
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                16F)},
            {Tuple.Create(Bandit.Oak, Difficulty.Merciless), Tuple.Create("+# to Maximum Endurance Charges",
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                1F)},
            {Tuple.Create(Bandit.Kraityn, Difficulty.Normal), Tuple.Create("#% to all Elemental Resistances",
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                10F)},
            {Tuple.Create(Bandit.Kraityn, Difficulty.Cruel), Tuple.Create("#% increased Attack Speed", 
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                8F)},
            {Tuple.Create(Bandit.Kraityn, Difficulty.Merciless), Tuple.Create("+# to Maximum Frenzy Charges", 
#	if (PoESkillTree_UseSmallDec_ForAttributes) 
            (SmallDec)
#	endif
                1F)},
        };
        /// <summary>
        /// Returns the reward attribute this bandit gives in this difficulty.
        /// </summary>
        /// <returns>The reward attribute. Null if the bandit has no reward (Bandit.None)./></returns>
#if (PoESkillTree_UseSmallDec_ForAttributes)
        public static Tuple<string, SmallDec> Reward(this Bandit bandit, Difficulty difficulty)
#else
        public static Tuple<string, float> Reward(this Bandit bandit, Difficulty difficulty)
#endif
        {
            return bandit == Bandit.None ? null : Rewards[Tuple.Create(bandit, difficulty)];
        }
    }

    /// <summary>
    /// Stores the bandit choice for each difficulty.
    /// </summary>
    public class BanditSettings : Notifier
    {
        private Bandit _normal;
        private Bandit _cruel;
        private Bandit _merciless;
        
        public Bandit Normal
        {
            get { return _normal; }
            set { SetProperty(ref _normal, value); }
        }
        
        public Bandit Cruel
        {
            get { return _cruel; }
            set { SetProperty(ref _cruel, value); }
        }
        
        public Bandit Merciless
        {
            get { return _merciless; }
            set { SetProperty(ref _merciless, value); }
        }

        [XmlIgnore]
#if (PoESkillTree_UseSmallDec_ForAttributes)
        public Dictionary<string, SmallDec> Rewards
#else
        public Dictionary<string, float> Rewards
#endif
        {
            get
            {
#if (PoESkillTree_UseSmallDec_ForAttributes)
                var rewards = new Dictionary<string, SmallDec>();
#else
                var rewards = new Dictionary<string, float>();
#endif
                if (Normal != Bandit.None)
                {
                    var r = Normal.Reward(Difficulty.Normal);
                    rewards[r.Item1] = r.Item2;
                }
                if (Cruel != Bandit.None)
                {
                    var r = Cruel.Reward(Difficulty.Cruel);
                    rewards[r.Item1] = r.Item2;
                }
                if (Merciless != Bandit.None)
                {
                    var r = Merciless.Reward(Difficulty.Merciless);
                    rewards[r.Item1] = r.Item2;
                }
                return rewards;
            }
        }

        public static IEnumerable<Bandit> BanditValues
        {
            get { return Enum.GetValues(typeof(Bandit)).Cast<Bandit>(); }
        }

        public void Reset()
        {
            Normal = Cruel = Merciless = Bandit.None;
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