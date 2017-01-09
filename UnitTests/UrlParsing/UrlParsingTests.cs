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

namespace UnitTests.UrlParsing
{
    [TestClass]
    public class UrlParsingTests
    {
        private static AbstractPersistentData _persistentData;
        private static SkillTree _tree;

        // These URLs represents same build from #427
        private static string PathofexileBuildUrl
            => "http://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

        private static string PoeplannerBuildUrl
            => "http://poeplanner.com/AAQAAPcTEAB534rpAkuuGyXZW20ZEVA1uacrHU-X9FJTPV-XlUyz0B_v6yBuUDARD6yY99eESFxrw23i9wFvi3oi9GwLkyd_xsHF7Bgsv4_60NBfakcG2-e0xcM6KjhcQFS9Fe2Wi9W5h8unm3WezxXYdrk-rGYs4X3j8uFjQ2wI8YrviPk3189Jsb6KoS8RlpAbcFaaE9vUieDmWJMfSRPK07QM6roNfBXXhq5WYw5IXfIqC3BSoqMPxDt8uMrw1UlROw32_Kc0YqxsjJ2qES_rY7VI1EKEb3X9Fr8PqySLNj1W9dtul9COihpI9Ch88JEHu_wVfn6hadgAAAAAAA==";

        private static int TotalPointsUsed => 113;

        // Used skill points +8 Asc points +1 for class root node +1 for ascendancy disk, represented by its own node
        private static int TotalNodesUsed => 123;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            AppData.SetApplicationData(Environment.CurrentDirectory);

            if (ItemDB.IsEmpty())
                ItemDB.Load("Data/ItemDB/GemList.xml", true);
            _persistentData = new BarePersistentData {CurrentBuild = new PoEBuild()};
            _persistentData.EquipmentData = EquipmentData.CreateAsync(_persistentData.Options).Result;

            // This initialization requires a lot of time, so it is reasonable to reuse one instance if possible.
            // However, as some tests may change tree state this field should be used only for methods,
            // that does not depend on tree instance.
            _tree = SkillTree.CreateAsync(_persistentData, null).Result;
        }

        [TestCleanup]
        public void Cleanup()
        {
            SkillTree.ClearAssets();
        }

        [TestMethod]
        public void DecodePathofexileUrlTest()
        {
            HashSet<SkillNode> nodes;
            int chartype;
            int asctype;
            SkillTree.DecodeUrl(PathofexileBuildUrl, out nodes, out chartype, out asctype);

            Assert.AreEqual(TotalNodesUsed, nodes.Count);
        }

        [TestMethod]
        [Ignore] // Fails. Described in the issue #427
        public void DecodePoeplannerUrlTest()
        {
            HashSet<SkillNode> nodes;
            int chartype;
            int asctype;
            SkillTree.DecodeUrl(PoeplannerBuildUrl, out nodes, out chartype, out asctype);

            Assert.AreEqual(TotalNodesUsed, nodes.Count);
        }

        [TestMethod]
        public async Task SaveTolTest()
        {
            string expectedUrl = PathofexileBuildUrl.Split('/').LastOrDefault();

            // Need instance created in current thread for a WPF UI logic
            SkillTree tree = await SkillTree.CreateAsync(_persistentData, null);
            tree.LoadFromUrl(PathofexileBuildUrl);

            var actualUrl = tree.SaveToUrl().Split('/').LastOrDefault();

            Assert.AreEqual(expectedUrl, actualUrl);

            SkillTree.ClearAssets();
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\UrlParsing\EmptyBuilds.xml", "TestBuild", DataAccessMethod.Sequential)]
        public void GetCharacterUrlTest()
        {
            string expectedUrl = Convert.ToString(TestContext.DataRow["TreeUrl"], CultureInfo.InvariantCulture);
            byte charType = Convert.ToByte(TestContext.DataRow["CharacterClassId"]);
            byte ascType = Convert.ToByte(TestContext.DataRow["AscendancyClassId"]);

            var actualUrl = SkillTree.GetCharacterUrl(charType, ascType);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public void PointsUsedTest()
        {
            uint actualCount = _tree.PointsUsed(PathofexileBuildUrl);

            Assert.AreEqual((uint)TotalPointsUsed, actualCount);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\UrlParsing\EmptyBuilds.xml", "TestBuild", DataAccessMethod.Sequential)]
        public void GetCharacterClassTest()
        {
            string expectedClass = Convert.ToString(TestContext.DataRow["Class"], CultureInfo.InvariantCulture);
            string targetUrl = Convert.ToString(TestContext.DataRow["TreeUrl"], CultureInfo.InvariantCulture);

            string actualClass = _tree.CharacterClass(Constants.TreeAddress + targetUrl);

            Assert.AreEqual(expectedClass, actualClass);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\UrlParsing\EmptyBuilds.xml", "TestBuild", DataAccessMethod.Sequential)]
        public void GetAscendancyClassTest()
        {
            // There are duplicate cases for rows with empty ascendancy class.
            // Consider this if test runs too slow.
            string expectedAscendancyClass = Convert.ToString(TestContext.DataRow["AscendancyClass"],
                CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(expectedAscendancyClass))
                expectedAscendancyClass = null;

            string targetUrl = Convert.ToString(TestContext.DataRow["TreeUrl"], CultureInfo.InvariantCulture);
            string actualAscendancyClass = _tree.AscendancyClass(Constants.TreeAddress + targetUrl);

            Assert.AreEqual(expectedAscendancyClass, actualAscendancyClass);
        }
    }
}