using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.GameModel.Tests.Skills
{
    [TestFixture]
    public class SupportabilityTesterTest
    {
        [TestCaseSource(nameof(CreateTestCases))]
        public IEnumerable<string> ReturnsCorrectResult(IEnumerable<string> supportSkillIds)
        {
            var activeSkill = new Skill("active", 20, 20, default, 0, 0);
            var supportSkills = supportSkillIds.Select((id, i) => new Skill(id, 20, 20, default, i + 1, 0));
            var sut = CreateSut();

            var actual = sut.SelectSupportingSkills(activeSkill, supportSkills);

            return actual.Select(s => s.Id).ToList();
        }

        private static IEnumerable<TestCaseData> CreateTestCases()
        {
            yield return CreateCase(("1empty", false));
            yield return CreateCase(("2allows0", true));
            yield return CreateCase(("3allows0excludes1", false));
            yield return CreateCase(("5allows3", true));
            yield return CreateCase(("4allows0adds4", true), ("6allows4", true));
            yield return CreateCase(("6allows4", true), ("9allows0adds4", true));
            yield return CreateCase(("4allows0adds4", true), ("10allows0excludes4", false));
            yield return CreateCase(("10allows0excludes4", false), ("11allows0excludes5adds4", true));
            yield return CreateCase(
                ("10allows0excludes4", false), ("11allows0excludes5adds4", true), ("13allows6adds5", false));
            yield return CreateCase(
                ("16allows0adds5", true), ("17allows0excludes5adds4", false), ("18allows4", false));
            // This is what occurs in-game. The first support is disabled by the second but allows the third to apply.
            // The logically expected result would be the same as the case above.
            yield return CreateCase(
                ("11allows0excludes5adds4", false), ("16allows0adds5", true), ("18allows4", true));
            // This situation can't exist in-game. Test for the result the in-game algorithm would probably produce
            // even if logically all supports should apply.
            yield return CreateCase(("7allows5", false), ("8allows4adds5", true), ("9allows0adds4", true));
            // There are no two support gems in-game where first.excluded_types and second.added_types, and
            // first.added_types and second.excluded_types both intersect. Therefore, the correct behavior can't be
            // determined.
            yield return CreateCase(("11allows0excludes5adds4", true), ("12allows0excludes4adds5", false));
            // Minion and normal types are checked together (verified in-game, PoB has incorrect behavior)
            yield return CreateCase(("19allows3excludes0", false));
            yield return CreateCase(("20allows0excludes3", false));

            TestCaseData CreateCase(params (string skillId, bool supporting)[] supports)
                => new TestCaseData(supports.Select(t => t.skillId).ToList())
                    .SetName(supports.Select(t => t.skillId).ToDelimitedString(", "))
                    .Returns(supports.Where(t => t.supporting).Select(t => t.skillId).ToList());
        }

        [Test]
        public void ThrowsIfSupportHasDifferentItemSlot()
        {
            var activeSkill = new Skill("active", 20, 20, ItemSlot.Boots, 0, 0);
            var supportSkills = new[] { new Skill("1empty", 20, 20, ItemSlot.Helm, 1, 0) };
            var sut = CreateSut();

            Assert.Throws<ArgumentException>(() => sut.SelectSupportingSkills(activeSkill, supportSkills));
        }

        [Test]
        public void ActiveSkillWithoutGemGroupCanBeSupportedByAllGemGroups()
        {
            var activeSkill = new Skill("active", 20, 20, default, 0, null);
            var supportSkills = new[]
            {
                new Skill("2allows0", 20, 20, default, 1, 0),
                new Skill("5allows3", 20, 20, default, 2, 1),
                new Skill("14allows1", 20, 20, default, 3, null),
            };
            var expected = supportSkills;
            var sut = CreateSut();

            var actual = sut.SelectSupportingSkills(activeSkill, supportSkills).ToList();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ActiveSkillWithGemGroupCanBeSupportedBySameOrNoGemGroup()
        {
            var activeSkill = new Skill("active", 20, 20, default, 0, 1);
            var supportSkills = new[]
            {
                new Skill("2allows0", 20, 20, default, 1, 0),
                new Skill("5allows3", 20, 20, default, 2, 1),
                new Skill("14allows1", 20, 20, default, 3, null),
            };
            var expected = new[] { supportSkills[1], supportSkills[2] };
            var sut = CreateSut();

            var actual = sut.SelectSupportingSkills(activeSkill, supportSkills).ToList();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ActiveSkillWithoutGemGroupIsNotSupportedByGemOnlySupports()
        {
            var activeSkill = new Skill("active", 20, 20, default, 0, null);
            var supportSkills = new[] { new Skill("21allows0gemsOnly", 20, 20, default, 1, 0) };
            var expected = new Skill[0];
            var sut = CreateSut();

            var actual = sut.SelectSupportingSkills(activeSkill, supportSkills).ToList();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ActiveSkillWithGemGroupIsSupportedByGemOnlySupports()
        {
            var activeSkill = new Skill("active", 20, 20, default, 0, 0);
            var supportSkills = new[] { new Skill("21allows0gemsOnly", 20, 20, default, 1, 0) };
            var expected = supportSkills;
            var sut = CreateSut();

            var actual = sut.SelectSupportingSkills(activeSkill, supportSkills).ToList();

            Assert.AreEqual(expected, actual);
        }

        private static SupportabilityTester CreateSut() => new SupportabilityTester(SkillDefinitions);

        private static SkillDefinitions SkillDefinitions => new SkillDefinitions(Skills);

        private static IReadOnlyList<SkillDefinition> Skills => new[]
        {
            SkillDefinition.CreateActive("active", 0, "", null, null,
                new ActiveSkillDefinition("active", 0, new[] { "0", "1", "2" }, new[] { "3" }, null, null, false, null,
                    null), null),
            CreateSupportDefinition("1empty", 1, false, new string[0], new string[0], new string[0]),
            CreateSupportDefinition("2allows0", 2, false, new[] { "0" }, new string[0], new string[0]),
            CreateSupportDefinition("3allows0excludes1", 3, false, new[] { "0" }, new[] { "1" }, new string[0]),
            CreateSupportDefinition("4allows0adds4", 4, false, new[] { "0" }, new string[0], new[] { "4" }),
            CreateSupportDefinition("5allows3", 5, false, new[] { "3" }, new string[0], new string[0]),
            CreateSupportDefinition("6allows4", 6, false, new[] { "4" }, new string[0], new string[0]),
            CreateSupportDefinition("7allows5", 7, false, new[] { "5" }, new string[0], new string[0]),
            CreateSupportDefinition("8allows4adds5", 8, false, new[] { "4" }, new string[0], new[] { "5" }),
            CreateSupportDefinition("9allows0adds4", 9, false, new[] { "0" }, new string[0], new[] { "4" }),
            CreateSupportDefinition("10allows0excludes4", 10, false, new[] { "0" }, new[] { "4" }, new string[0]),
            CreateSupportDefinition("11allows0excludes5adds4", 11, false, new[] { "0" }, new[] { "5" }, new[] { "4" }),
            CreateSupportDefinition("12allows0excludes4adds5", 12, false, new[] { "0" }, new[] { "4" }, new[] { "5" }),
            CreateSupportDefinition("13allows6adds5", 13, false, new[] { "6" }, new string[0], new[] { "5" }),
            CreateSupportDefinition("14allows1", 14, false, new[] { "1" }, new string[0], new string[0]),
            CreateSupportDefinition("16allows0adds5", 16, false, new[] { "0" }, new string[0], new[] { "5" }),
            CreateSupportDefinition("17allows0excludes5adds4", 17, false, new[] { "0" }, new[] { "5" }, new[] { "4" }),
            CreateSupportDefinition("18allows4", 18, false, new[] { "4" }, new string[0], new string[0]),
            CreateSupportDefinition("19allows3excludes0", 19, false, new[] { "3" }, new[] { "0" }, new string[0]),
            CreateSupportDefinition("20allows0excludes3", 20, false, new[] { "0" }, new[] { "3" }, new string[0]),
            CreateSupportDefinition("21allows0gemsOnly", 21, true, new[] { "0" }, new string[0], new string[0]),
        };

        private static SkillDefinition CreateSupportDefinition(
            string id, int numericId, bool supportsGemsOnly, IEnumerable<string> allowedActiveSkillTypes,
            IEnumerable<string> excludedActiveSkillTypes, IEnumerable<string> addedActiveSkillTypes)
            => SkillDefinition.CreateSupport(id, numericId, "", null, null,
                new SupportSkillDefinition(supportsGemsOnly, allowedActiveSkillTypes, excludedActiveSkillTypes,
                    addedActiveSkillTypes, null), null);
    }
}