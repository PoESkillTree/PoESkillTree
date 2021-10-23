using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils.UrlProcessing.Decoders;

namespace PoESkillTree.Utils.UrlProcessing
{
    [TestFixture]
    public class BuildUrlDeserializerTest
    {
        [TestCaseSource(nameof(CreateNodeCountData))]
        public int GetBuildDataReturnsCorrectAmountOfNodes(string url)
        {
            var sut = CreateSut(url);

            return sut.SkilledNodesIds.Count;
        }

        private static IEnumerable<TestCaseData> CreateNodeCountData()
        {
            yield return new TestCaseData(
                    "https://www.pathofexile.com/passive-skill-tree/AAAABgMBeQFvDXwOSA-rD8QRDxFQEZYV1xXtFr8aSBslHU8gbiL0JIsqCyy_LOE2PTdNOw07fD1fRwZJUUmxS65Ms1AwUEJSU1M1VL1W9VxAXGtcil3yX2pirGNDadhsCGwLbIxtGXBScFZ1nnX9fPB-oX_Gg_eESIRvhq6Hy4t6joqPRo_6kQeSdJMfkyeWi5eVl9CX9JoTm6GdqqEvoqOnCKcrpzSnm6xmrJivvLQMtMW1BLVIuMq5Ark-u_y-isHFw23K088V0B_Q0NRC1bnXz9h22VvbbtvU2-ffiuZY6QLquutj7BjviO_r8NXxivQo9vz31_k3AAA=")
                .Returns(121);
        }

        [TestCaseSource(nameof(CreateEmptyBuildData))]
        public void GetCharacterClassReturnsCorrectResult(string url, CharacterClass expected, int _)
        {
            var sut = CreateSut(url);

            var actual = sut.CharacterClass;

            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(CreateEmptyBuildData))]
        public void GetAscendancyClassIdReturnsCorrectResult(string url, CharacterClass _, int expected)
        {
            var sut = CreateSut(url);

            var actual = sut.AscendancyClassId;

            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<TestCaseData> CreateEmptyBuildData()
        {
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgAAAAAA",
                CharacterClass.Scion, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgABAAAA",
                CharacterClass.Scion, 1);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgEAAAAA",
                CharacterClass.Marauder, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgEBAAAA",
                CharacterClass.Marauder, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgECAAAA",
                CharacterClass.Marauder, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgEDAAAA",
                CharacterClass.Marauder, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgIAAAAA",
                CharacterClass.Ranger, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgIBAAAA",
                CharacterClass.Ranger, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgICAAAA",
                CharacterClass.Ranger, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgIDAAAA",
                CharacterClass.Ranger, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgMAAAAA",
                CharacterClass.Witch, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgMBAAAA",
                CharacterClass.Witch, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgMCAAAA",
                CharacterClass.Witch, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgMDAAAA",
                CharacterClass.Witch, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgQAAAAA",
                CharacterClass.Duelist, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgQBAAAA",
                CharacterClass.Duelist, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgQCAAAA",
                CharacterClass.Duelist, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgQDAAAA",
                CharacterClass.Duelist, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgUAAAAA",
                CharacterClass.Templar, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgUBAAAA",
                CharacterClass.Templar, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgUCAAAA",
                CharacterClass.Templar, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgUDAAAA",
                CharacterClass.Templar, 3);

            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgYAAAAA",
                CharacterClass.Shadow, 0);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgYBAAAA",
                CharacterClass.Shadow, 1);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgYCAAAA",
                CharacterClass.Shadow, 2);
            yield return new TestCaseData("https://www.pathofexile.com/passive-skill-tree/AAAABgYDAAAA",
                CharacterClass.Shadow, 3);
        }

        private static SkillTreeUrlData CreateSut(string url)
        {
            return SkillTreeUrlDecoders.TryDecode(url);
        }
    }
}