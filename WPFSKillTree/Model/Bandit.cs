using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    /// <summary>
    /// Enumeration of the bandits from whom can be choose at each difficulty.
    /// (chosing none gives an extra skill point)
    /// </summary>
    public enum Bandit
    {
        None,
        Alira,
        Oak,
        Kraityn
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
        private static readonly Dictionary<Tuple<Bandit, Difficulty>, Tuple<string, float>> Rewards = new Dictionary<Tuple<Bandit, Difficulty>, Tuple<string, float>>
        {
            {Tuple.Create(Bandit.Alira, Difficulty.Normal), Tuple.Create("+# to maximum Mana", 60F)},
            {Tuple.Create(Bandit.Alira, Difficulty.Cruel), Tuple.Create("#% increased Cast Speed", 5F)},
            {Tuple.Create(Bandit.Alira, Difficulty.Merciless), Tuple.Create("+# to Maximum Power Charges", 1F)},
            {Tuple.Create(Bandit.Oak, Difficulty.Normal), Tuple.Create("+# to maximum Life", 40F)},
            {Tuple.Create(Bandit.Oak, Difficulty.Cruel), Tuple.Create("#% increased Physical Damage", 16F)},
            {Tuple.Create(Bandit.Oak, Difficulty.Merciless), Tuple.Create("+# to Maximum Endurance Charges", 1F)},
            {Tuple.Create(Bandit.Kraityn, Difficulty.Normal), Tuple.Create("#% to all Elemental Resistances", 10F)},
            {Tuple.Create(Bandit.Kraityn, Difficulty.Cruel), Tuple.Create("#% increased Attack Speed", 8F)},
            {Tuple.Create(Bandit.Kraityn, Difficulty.Merciless), Tuple.Create("+# to Maximum Frenzy Charges", 1F)},
        };

        /// <summary>
        /// Returns the reward attribute this bandit gives in this difficulty.
        /// </summary>
        /// <returns>The reward attribute. Null iff the bandit has no reward (Bandit.None)./></returns>
        public static Tuple<string, float> Reward(this Bandit bandit, Difficulty difficulty)
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
        public Dictionary<string, float> Rewards
        {
            get
            {
                var rewards = new Dictionary<string, float>();
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

        public BanditSettings Clone()
        {
            return new BanditSettings
            {
                Normal = Normal,
                Cruel = Cruel,
                Merciless = Merciless
            };
        }
    }
}