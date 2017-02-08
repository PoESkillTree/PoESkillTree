using System.Xml.Serialization;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Serializable bandits test settings.
    /// </summary>
    public class TestBanditSettings
    {
        /// <summary>
        /// Represents poeplanner bandit Ids.
        /// </summary>
        // TODO: Poeplanner has its own ids, so it is unreliable to use existing enum. Need clarification.
        public enum PoeplannerBandit
        {
            None = 0,
            Alira = 1,
            Kraityn = 2,
            Oak = 3
        }

        [XmlAttribute("normal")]
        public PoeplannerBandit Normal { get; set; }
        [XmlAttribute("cruel")]
        public PoeplannerBandit Cruel { get; set; }
        [XmlAttribute("merciless")]
        public PoeplannerBandit Merciless { get; set; }
    }
}