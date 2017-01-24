using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.Model.Serialization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.Utils.UrlProcessing;
using UnitTests.TestBuilds.Utils;

namespace UnitTests.UrlProcessing
{
    [TestClass]
    public class UrlProcessingTests
    {
        private static AbstractPersistentData _persistentData;
        private static BuildUrlCollection _builds;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            AppData.SetApplicationData(Environment.CurrentDirectory);

            if (ItemDB.IsEmpty())
                ItemDB.Load("Data/ItemDB/GemList.xml", true);
            _persistentData = new BarePersistentData { CurrentBuild = new PoEBuild() };
            _persistentData.EquipmentData = EquipmentData.CreateAsync(_persistentData.Options).Result;

            _builds = TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/BuildUrls.xml");
            _builds.AddRange(TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/EmptyBuildUrls.xml"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            SkillTree.ClearAssets();
        }

        #region Full tree selected

        [TestMethod]
        public async Task FullPathofexileTreeLoadUnloadTest()
        {
            var build = _builds.FindByName("PoeplannerFullTreeWithMaxScionAndBandits");
            var targetUrl = build.GetAlternativeUrl("pathofexile");

            SkillTree.ClearAssets();
            SkillTree tree = await SkillTree.CreateAsync(_persistentData);
            tree.LoadFromUrl(targetUrl);

            var actualUrl = new SkillTreeSerializer(tree).ToUrl();

            Assert.AreEqual(targetUrl, actualUrl);
        }

        [TestMethod]
        public async Task FullPoeplannerTreeLoadUnloadTest()
        {
            var build = _builds.FindByName("PoeplannerFullTreeWithMaxScionAndBandits");

            SkillTree.ClearAssets();
            SkillTree tree = await SkillTree.CreateAsync(_persistentData);
            tree.LoadFromUrl(build.DefaultUrl);

            var actualUrl = new SkillTreeSerializer(tree).ToUrl();

            Assert.AreEqual(build.GetAlternativeUrl("pathofexile"), actualUrl);
        }

        #endregion

        #region Urls decoding - pathofexile.com

        [TestMethod]
        public void DecodePathofexileUrlTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");

            var nodes = DecodeInSkillTree(build.DefaultUrl);

            Assert.AreEqual(build.GetTotalPoints(true), nodes.Count);
        }

        #endregion

        #region Urls decoding - poeplanner.com

        [TestMethod]
        public void DecodePoeplannerUrlTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");

            var nodes = DecodeInSkillTree(build.DefaultUrl);

            Assert.AreEqual(build.GetTotalPoints(true), nodes.Count);
        }

        [TestMethod]
        public void DecodePoeplannerUrlWithJewelsTest()
        {
            var build = _builds.FindByName("PoeplannerSmallWitchOccultistWithOneJewel");

            var nodes = DecodeInSkillTree(build.DefaultUrl);

            Assert.AreEqual(build.GetTotalPoints(true), nodes.Count);
        }

        [TestMethod]
        public void DecodePoedbUrlTest()
        {
            var build = _builds.FindByName("PoedbBuild");

            var nodes = DecodeInSkillTree(build.DefaultUrl);

            Assert.AreEqual(build.GetTotalPoints(true), nodes.Count);
        }

        [TestMethod]
        public void DecodePoeplannerUrlWithAurasTest()
        {
            var build = _builds.FindByName("PoeplannerSmallScionWithAuras");

            var nodes = DecodeInSkillTree(build.DefaultUrl);

            Assert.AreEqual(1, nodes.Count, "Only one class root node expected.");
        }

        [TestMethod]
        public void DecodePoeplannerUrlWithEquipmentTest()
        {
            var build = _builds.FindByName("PoeplannerSmallScionWithUquip");

            var nodes = DecodeInSkillTree(build.DefaultUrl);

            Assert.AreEqual(1, nodes.Count, "Only one class root node expected.");
        }

        #endregion

        #region Urls encoding

        [TestMethod]
        public async Task SaveToUrlTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var targetUrl = build.GetAlternativeUrl("pathofexileWindowed");

            string expectedUrl = targetUrl.Split('/').LastOrDefault();

            // Need instance created in current thread for a WPF UI logic
            SkillTree.ClearAssets();
            SkillTree tree = await SkillTree.CreateAsync(_persistentData);
            tree.LoadFromUrl(targetUrl);

            var actualUrl = tree.Serializer.ToUrl().Split('/').LastOrDefault();

            Assert.AreEqual(expectedUrl, actualUrl);

            SkillTree.ClearAssets();
        }

        [TestMethod]
        public async Task SaveToUrlNoAscendancyPointsTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultist");
            var targetUrl = build.DefaultUrl;

            string expectedSegment = targetUrl.Split('/').LastOrDefault();

            // Need new instance created in current thread for a WPF UI logic
            SkillTree.ClearAssets();
            SkillTree tree = await SkillTree.CreateAsync(_persistentData);
            tree.LoadFromUrl(targetUrl);

            var actualSegment = tree.Serializer.ToUrl().Split('/').LastOrDefault();

            Assert.AreEqual(expectedSegment, actualSegment);

            SkillTree.ClearAssets();
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
        public void PointsUsedTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var targetUrl = build.GetAlternativeUrl("pathofexileWindowed");

            var actualCount = BuildConverter.GetUrlDeserializer(targetUrl).GetPointsCount();

            Assert.AreEqual(build.Points, actualCount);
        }

        [TestMethod]
        public void PointsUsedWithAscendancyTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var targetUrl = build.GetAlternativeUrl("pathofexileWindowed");

            var actualCount = BuildConverter.GetUrlDeserializer(targetUrl).GetPointsCount(true);

            Assert.AreEqual(build.GetTotalPoints(), actualCount);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\TestBuilds\EmptyBuildUrls.xml", "build", DataAccessMethod.Sequential)]
        public void GetCharacterClassTest()
        {
            var expectedClassId = Convert.ToInt32(TestContext.DataRow["characterClassId"]);
            var expectedClass = Enum.GetName(typeof(CharacterClasses), expectedClassId);
            var targetUrl = Convert.ToString(TestContext.DataRow.GetChildRows("build_urls")[0]["default"], CultureInfo.InvariantCulture);

            var actualClass =
                BuildConverter.GetUrlDeserializer(Constants.TreeAddress + targetUrl).GetCharacterClass();

            Assert.AreEqual(expectedClass, actualClass);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\TestBuilds\EmptyBuildUrls.xml", "build", DataAccessMethod.Sequential)]
        public void GetAscendancyClassTest()
        {
            // There are duplicate cases for rows with empty ascendancy class.
            // Consider this if test runs too slow.
            var characterClassId = Convert.ToInt32(TestContext.DataRow["characterClassId"]);
            var ascendancyClassId = Convert.ToInt32(TestContext.DataRow["ascendancyClassId"]);

            string expectedAscendancyClass = ascendancyClassId > 0
                ? AscendancyClasses.GetClassName(characterClassId, ascendancyClassId)
                : null;

            string targetUrl = Convert.ToString(TestContext.DataRow.GetChildRows("build_urls")[0]["default"], CultureInfo.InvariantCulture);
            string actualAscendancyClass =
                BuildConverter.GetUrlDeserializer(targetUrl).GetAscendancyClass();

            Assert.AreEqual(expectedAscendancyClass, actualAscendancyClass);
        }

        [TestMethod]
        public async Task GetPointCountTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var targetUrl = build.GetAlternativeUrl("pathofexileWindowed");

            // Need instance created in current thread for a WPF UI logic
            SkillTree.ClearAssets();
            SkillTree tree = await SkillTree.CreateAsync(_persistentData);
            tree.LoadFromUrl(targetUrl);

            var deserializer = BuildConverter.GetUrlDeserializer(targetUrl);

            var points = tree.GetPointCount();
            var count = points["NormalUsed"] + points["AscendancyUsed"] + points["ScionAscendancyChoices"];

            Assert.AreEqual(count, deserializer.GetPointsCount(true));
        }

        #endregion

        #region Helpers

        private static HashSet<SkillNode> DecodeInSkillTree(string buildUrl)
        {
            HashSet<SkillNode> nodes;
            SkillTree.DecodeUrl(buildUrl, out nodes);
            return nodes;
        }

        #endregion
    }
}