using System.Xml.Serialization;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Represents root element of xml file, containing build Urls.
    /// </summary>
    [XmlRoot("testData")]
    public class TestData
    {
        [XmlArray("builds")]
        [XmlArrayItem("build", typeof(BuildUrlTestModel))]
        public BuildUrlCollection Builds { get; set; }
    }
}