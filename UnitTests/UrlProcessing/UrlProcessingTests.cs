using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PoESkillTree.GameModel;
using POESKillTree.Utils.UrlProcessing;
using UnitTests.TestBuilds.Utils;

namespace UnitTests.UrlProcessing
{
    [TestClass]
    public class UrlProcessingTests
    {
        private static BuildUrlCollection _builds;
        private static BuildConverter _buildConverter;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _builds = TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/BuildUrls.xml");
            _builds.AddRange(TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/EmptyBuildUrls.xml"));

            _buildConverter = new BuildConverter(null);
            _buildConverter.RegisterDefaultDeserializer(url => new NaivePoEUrlDeserializer(url, null));
            _buildConverter.RegisterDeserializersFactories(
                PoeplannerUrlDeserializer.TryCreate,
                PathofexileUrlDeserializer.TryCreate
            );
        }

        #region Full tree selected

        [TestMethod]
        public void FullPathofexileTreeLoadUnloadTest()
        {
            var build = _builds.FindByName("PoeplannerFullTreeWithMaxScionAndBandits");
            var targetUrl = build.GetAlternativeUrl("pathofexile");

            var actualUrl = Serialize(targetUrl);

            Assert.AreEqual(targetUrl, actualUrl);
        }

        [TestMethod]
        public void FullPoeplannerTreeLoadUnloadTest()
        {
            var build = _builds.FindByName("PoeplannerFullTreeWithMaxScionAndBandits");

            var actualUrl = Serialize(build.DefaultUrl);

            Assert.AreEqual(build.GetAlternativeUrl("pathofexile"), actualUrl);
        }

        #endregion

        #region Urls decoding - pathofexile.com

        [TestMethod]
        public void DecodePathofexileUrlTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        #endregion

        #region Urls decoding - poeplanner.com

        [TestMethod]
        public void DecodePoeplannerUrlTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        [TestMethod]
        public void DecodePoeplannerUrlWithJewelsTest()
        {
            var build = _builds.FindByName("PoeplannerSmallWitchOccultistWithOneJewel");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        [TestMethod]
        public void DecodePoedbUrlTest()
        {
            var build = _builds.FindByName("PoedbBuild");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        [TestMethod]
        public void DecodePoeplannerUrlWithAurasTest()
        {
            var build = _builds.FindByName("PoeplannerSmallScionWithAuras");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        [TestMethod]
        public void DecodePoeplannerUrlWithEquipmentTest()
        {
            var build = _builds.FindByName("PoeplannerSmallScionWithUquip");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        [TestMethod]
        public void DecodeObsoleteBuildTest()
        {
            var build = _builds.FindByName("ObsoleteTemplar");

            var nodes = Deserialize(build.DefaultUrl);

            Assert.AreEqual(build.Nodes, nodes.Count);
        }

        #endregion

        #region Urls encoding

        [TestMethod]
        public void SaveToUrlTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var targetUrl = build.GetAlternativeUrl("pathofexileWindowed");

            string expectedUrl = targetUrl.Split('/').LastOrDefault();

            var actualUrl = Serialize(targetUrl).Split('/').LastOrDefault();

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public void SaveToUrlNoAscendancyPointsTest()
        {
            var build = _builds.FindByName("PathofexilWitchOccultist");
            var targetUrl = build.DefaultUrl;

            string expectedSegment = targetUrl.Split('/').LastOrDefault();

            var actualSegment = Serialize(targetUrl).Split('/').LastOrDefault();

            Assert.AreEqual(expectedSegment, actualSegment);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\TestBuilds\EmptyBuildUrls.xml", "build", DataAccessMethod.Sequential)]
        public void GetEmptyBuildUrlTest()
        {
            string expectedUrl =
                Convert.ToString(TestContext.DataRow.GetChildRows("build_urls")[0]["default"],
                    CultureInfo.InvariantCulture).Split('/').LastOrDefault();
            byte charType = Convert.ToByte(TestContext.DataRow["characterClassId"]);
            byte ascType = Convert.ToByte(TestContext.DataRow["ascendancyClassId"]);

            var actualUrl = SkillTreeSerializer.GetEmptyBuildUrl(charType, ascType);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        #endregion

        #region Misc

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\TestBuilds\EmptyBuildUrls.xml", "build", DataAccessMethod.Sequential)]
        public void GetCharacterClassTest()
        {
            var expectedClass = (CharacterClass) Convert.ToInt32(TestContext.DataRow["characterClassId"]);
            var targetUrl = Convert.ToString(TestContext.DataRow.GetChildRows("build_urls")[0]["default"], CultureInfo.InvariantCulture);

            var actualClass = _buildConverter.GetUrlDeserializer(targetUrl).GetCharacterClass();

            Assert.AreEqual(expectedClass, actualClass);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\TestBuilds\EmptyBuildUrls.xml", "build", DataAccessMethod.Sequential)]
        public void GetAscendancyClassTest()
        {
            // There are duplicate cases for rows with empty ascendancy class.
            // Consider this if test runs too slow.
            var expectedAscendancyClass = Convert.ToInt32(TestContext.DataRow["ascendancyClassId"]);
            string targetUrl = Convert.ToString(TestContext.DataRow.GetChildRows("build_urls")[0]["default"], CultureInfo.InvariantCulture);

            var actualAscendancyClass = _buildConverter.GetUrlDeserializer(targetUrl).GetAscendancyClassId();

            Assert.AreEqual(expectedAscendancyClass, actualAscendancyClass);
        }

        #endregion

        #region Helpers

        private static IReadOnlyCollection<ushort> Deserialize(string buildUrl)
            => _buildConverter.GetUrlDeserializer(buildUrl).GetBuildData().SkilledNodesIds;

        private static string Serialize(string buildUrl)
        {
            var buildData = _buildConverter.GetUrlDeserializer(buildUrl).GetBuildData();
            var allNodes = Mock.Of<ICollection<ushort>>(c => c.Contains(It.IsAny<ushort>()));
            return new SkillTreeSerializer(buildData, allNodes).ToUrl();
        }

        #endregion
    }
}