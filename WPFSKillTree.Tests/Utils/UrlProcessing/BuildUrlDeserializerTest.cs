using System.Collections.Generic;
using NUnit.Framework;
using PoESkillTree.GameModel;
using PoESkillTree.Utils.UrlProcessing;

namespace PoESkillTree.Tests.Utils.UrlProcessing
{
    [TestFixture]
    public class BuildUrlDeserializerTest
    {
        [TestCaseSource(nameof(CreateNodeCountData))]
        public int GetBuildDataReturnsCorrectAmountOfNodes(string url)
        {
            var sut = CreateSut(url);

            return sut.GetBuildData().SkilledNodesIds.Count;
        }

        private static IEnumerable<TestCaseData> CreateNodeCountData()
        {
            yield return new TestCaseData(
                    "http://poeplanner.com/AAQAAPcTEAB534rpAkuuGyXZW20ZEVA1uacrHU-X9FJTPV-XlUyz0B_v6yBuUDARD6yY99eESFxrw23i9wFvi3oi9GwLkyd_xsHF7Bgsv4_60NBfakcG2-e0xcM6KjhcQFS9Fe2Wi9W5h8unm3WezxXYdrk-rGYs4X3j8uFjQ2wI8YrviPk3189Jsb6KoS8RlpAbcFaaE9vUieDmWJMfSRPK07QM6roNfBXXhq5WYw5IXfIqC3BSoqMPxDt8uMrw1UlROw32_Kc0YqxsjJ2qES_rY7VI1EKEb3X9Fr8PqySLNj1W9dtul9COihpI9Ch88JEHu_wVfn6hadgAAAAAAA==")
                .Returns(121);
            yield return new TestCaseData(
                    "https://pathofexile.com/fullscreen-passive-skill-tree/AAAABAMBAQFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3")
                .Returns(121);
            yield return new TestCaseData(
                    "http://poeplanner.com/AAQAACITAAAI37CSwY6-ES18g-vuHNyPGgGPGgoCFQIAAAEAAAAAAAAAAA==")
                .Returns(8);
            yield return new TestCaseData(
                    "http://poedb.tw/us/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3")
                .Returns(121);
            yield return new TestCaseData(
                    "http://poeplanner.com/AAQCAAAAIgEBAQACAAfQAQH0AQkACxEADBAACwUJBw0KAwAABwEABgIAAA==")
                .Returns(0);
            yield return new TestCaseData(
                    "http://poeplanner.com/AAQBAAAAAAAkAAAACQDOABQAAAQHAAwAAAAUAQF4AAADAwAAAAAAAAAAAAAA")
                .Returns(0);
            yield return new TestCaseData(
                    "https://poebuilder.com/character/AAAAAgUAfA6nVVugQ8hPBGjyogDwH5o74XPvfDwFgpuQVScv7Dhj_XrmcYXAGho4RtexQtq51abtPBps5CI26eNqFm8kqnF5FxwSaZ2upwismPFs7FUqjUkbNtiD21XGOlhMswQHPV9SU5eVwfOVIC0f2wuCHl8q6-QRln_GoqO3dR7wDkjljp1jXfKMNqSxJpVh4lVLFr9Om9ngm7VTNQ-rRitTUkWdmuDr7riT-ejYvf4Ktz5Bh9gkNsUfAodlr7cyNDwtxq7E9gn21HwGDiftU6UMXxyn7hVJUfDVlS4HHoLHxp4Qfwx9KPo=")
                .Returns(115);
        }

        [TestCaseSource(nameof(CreateEmptyBuildData))]
        public void GetCharacterClassReturnsCorrectResult(string url, CharacterClass expected, int _)
        {
            var sut = CreateSut(url);

            var actual = sut.GetCharacterClass();

            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(CreateEmptyBuildData))]
        public void GetAscendancyClassIdReturnsCorrectResult(string url, CharacterClass _, int expected)
        {
            var sut = CreateSut(url);

            var actual = sut.GetAscendancyClassId();

            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<TestCaseData> CreateEmptyBuildData()
        {
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==",
                CharacterClass.Scion, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAABAA==",
                CharacterClass.Scion, 1);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAEAAA==",
                CharacterClass.Marauder, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAEBAA==",
                CharacterClass.Marauder, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAECAA==",
                CharacterClass.Marauder, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAEDAA==",
                CharacterClass.Marauder, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAIAAA==",
                CharacterClass.Ranger, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAIBAA==",
                CharacterClass.Ranger, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAICAA==",
                CharacterClass.Ranger, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAIDAA==",
                CharacterClass.Ranger, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAMAAA==",
                CharacterClass.Witch, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAMBAA==",
                CharacterClass.Witch, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAMCAA==",
                CharacterClass.Witch, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAMDAA==",
                CharacterClass.Witch, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAQAAA==",
                CharacterClass.Duelist, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAQBAA==",
                CharacterClass.Duelist, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAQCAA==",
                CharacterClass.Duelist, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAQDAA==",
                CharacterClass.Duelist, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAUAAA==",
                CharacterClass.Templar, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAUBAA==",
                CharacterClass.Templar, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAUCAA==",
                CharacterClass.Templar, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAUDAA==",
                CharacterClass.Templar, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAYAAA==",
                CharacterClass.Shadow, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAYBAA==",
                CharacterClass.Shadow, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAYCAA==",
                CharacterClass.Shadow, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABAYDAA==",
                CharacterClass.Shadow, 3);
        }

        private static BuildUrlDeserializer CreateSut(string url)
        {
            var buildConverter = new BuildConverter(null);
            buildConverter.RegisterDeserializersFactories(
                PathofexileUrlDeserializer.TryCreate,
                PoeplannerUrlDeserializer.TryCreate);
            buildConverter.RegisterDefaultDeserializer(u => new NaivePoEUrlDeserializer(u, null));
            return buildConverter.GetUrlDeserializer(url);
        }
    }
}