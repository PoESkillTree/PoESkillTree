using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.TestBuilds.Utils;

namespace UnitTests.UrlProcessing
{
    /// <summary>
    /// Test helper tests.
    /// </summary>
    [TestClass]
    public class BuildLoadingTests
    {
        [TestMethod]
        public void LoadFromFileTest()
        {
            var builds = TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/BuildUrls.xml");

            Assert.IsNotNull(builds);
            Assert.IsTrue(builds.Any());

            var build = builds.FindByName("PoeplannerWitchOccultistAscendant");

            Assert.AreEqual(
                "Original build from the issue #427.The build is valid and contains only nodes information (no equip or auras).",
                build.Description);
            Assert.AreEqual(4, build.TreeVersion);
            Assert.AreEqual(3, build.CharacterClassId);
            Assert.AreEqual(1, build.AscendancyClassId);
            Assert.AreEqual(113, build.Points);
            Assert.AreEqual(8, build.AscendancyPoints);

            Assert.IsNotNull(build.Urls);
            Assert.IsTrue(build.Urls.Any());

            Assert.AreEqual(
                "http://poeplanner.com/AAQAAPcTEAB534rpAkuuGyXZW20ZEVA1uacrHU-X9FJTPV-XlUyz0B_v6yBuUDARD6yY99eESFxrw23i9wFvi3oi9GwLkyd_xsHF7Bgsv4_60NBfakcG2-e0xcM6KjhcQFS9Fe2Wi9W5h8unm3WezxXYdrk-rGYs4X3j8uFjQ2wI8YrviPk3189Jsb6KoS8RlpAbcFaaE9vUieDmWJMfSRPK07QM6roNfBXXhq5WYw5IXfIqC3BSoqMPxDt8uMrw1UlROw32_Kc0YqxsjJ2qES_rY7VI1EKEb3X9Fr8PqySLNj1W9dtul9COihpI9Ch88JEHu_wVfn6hadgAAAAAAA==",
                build.DefaultUrl);

            Assert.AreEqual("pathofexile", build.Urls[1].Name);

            Assert.IsNotNull(build.Tags);
            Assert.IsTrue(build.Tags.Any());

            Assert.IsNotNull(build.Bandits);
            Assert.AreEqual(TestBanditSettings.PoeplannerBandit.None, build.Bandits.Normal);
            Assert.AreEqual(TestBanditSettings.PoeplannerBandit.None, build.Bandits.Cruel);
            Assert.AreEqual(TestBanditSettings.PoeplannerBandit.Alira, build.Bandits.Merciless);
        }

        [TestMethod]
        public void FindByTagTest()
        {
            var builds = TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/BuildUrls.xml");

            var models = builds.FindByTag("bandits");

            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count >= 3);
        }

        [TestMethod]
        public void FindByTagsTest()
        {
            var builds = TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/BuildUrls.xml");

            var models = builds.FindByTags("jewels", "auras", "equip");

            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count >= 3);
        }
    }
}
