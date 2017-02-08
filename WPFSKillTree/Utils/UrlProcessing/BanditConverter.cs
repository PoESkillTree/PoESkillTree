using System;
using POESKillTree.Model;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Provides functionality for converting bandits identifiers from external sources into used in the POESKillTree application.
    /// </summary>
    public class BanditConverter
    {
        private readonly Func<int?, Bandit> _converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BanditConverter"/> class.
        /// </summary>
        /// <param name="converter">The delegate that accepts external id and returns <see cref="Bandit"/> value.</param>
        protected BanditConverter(Func<int?, Bandit> converter)
        {
            _converter = converter;
        }

        /// <summary>
        /// Default converter.
        /// </summary>
        public static BanditConverter Default => new BanditConverter(id => (Bandit)(id ?? 0));

        /// <summary>
        /// Poeplanner converter.
        /// </summary>
        public static BanditConverter PoEPlanner => new BanditConverter(id =>
        {
            switch (id ?? 0)
            {
                case 1:
                    return Bandit.Alira;
                case 2:
                    return Bandit.Kraityn;
                case 3:
                    return Bandit.Oak;
                default:
                    return Bandit.None;
            }
        });

        /// <summary>
        /// Converts specified <paramref name="id"/> into a <see cref="Bandit"/> value.<para/>
        /// Different planners may use different identifiers, incompatible with used in application.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Bandit GetBandit(int? id)
        {
            return _converter(id);
        }
    }
}