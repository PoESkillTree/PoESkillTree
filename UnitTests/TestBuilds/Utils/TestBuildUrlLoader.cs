using System.Xml;
using System.Xml.Serialization;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Class for deserializing build Urls from xml.
    /// </summary>
    internal static class TestBuildUrlLoader
    {
        /// <summary>
        /// Loads build Urls collection.
        /// </summary>
        /// <param name="filePath">The xml file to load.</param>
        /// <returns></returns>
        public static BuildUrlCollection LoadFromXmlFile(string filePath)
        {
            var serializer = new XmlSerializer(typeof(TestData));
            var data = (TestData)serializer.Deserialize(new XmlTextReader(filePath));

            return data.Builds;
        }
    }
}