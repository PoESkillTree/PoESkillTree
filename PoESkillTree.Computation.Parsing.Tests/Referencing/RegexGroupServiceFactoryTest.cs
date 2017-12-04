using System;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Referencing;
using static PoESkillTree.Computation.Parsing.Referencing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class RegexGroupServiceFactoryTest
    {
        [Test]
        public void SutIsIRegexGroupFactory()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IRegexGroupFactory>(sut);
        }

        [TestCase("left", "right", ExpectedResult = "left_right")]
        [TestCase("l", "r", ExpectedResult = "l_r")]
        [TestCase("", "r", ExpectedResult = "r")]
        public string CombineGroupPrefixesReturnsCorrectResult(string left, string right)
        {
            var sut = CreateSut();

            return sut.CombineGroupPrefixes(left, right);
        }

        [Test]
        public void CombineGroupPrefixesThrowsIfRightIsEmpty()
        {
            var sut = CreateSut();

            Assert.Throws<ArgumentException>(() => sut.CombineGroupPrefixes("left", ""));
        }

        [TestCase("", "innerRegex", ExpectedResult = "(?<" + ValueGroupPrefix + ">innerRegex)")]
        [TestCase("prefix", "inner", ExpectedResult = "(?<" + ValueGroupPrefix + "prefix>inner)")]
        public string CreateValueGroupReturnsCorrectResult(string groupNameIdentifier, string innerRegex)
        {
            var sut = CreateSut();

            return sut.CreateValueGroup(groupNameIdentifier, innerRegex);
        }

        [Test]
        public void CreateValueGroupThrowsIfInnerRegexIsEmpty()
        {
            var sut = CreateSut();

            Assert.Throws<ArgumentException>(() => sut.CreateValueGroup("id", ""));
        }

        [TestCase("groupPrefix", "innerRegex", "referenceName", 42)]
        [TestCase("", "inner", "name", 5)]
        public void CreateReferenceGroupReturnsCorrectResult(
            string groupPrefix, string innerRegex, string referenceName, int matcherIndex)
        {
            var expected =
                $"(?<{ReferenceGroupPrefix}{groupPrefix}_{referenceName}_{matcherIndex}>{innerRegex})";
            var sut = CreateSut();

            var actual = sut.CreateReferenceGroup(groupPrefix, referenceName, matcherIndex, innerRegex);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CreateReferenceGroupThrowsIfInnerRegexIsEmpty()
        {
            var sut = CreateSut();

            Assert.Throws<ArgumentException>(() => sut.CreateReferenceGroup("id", "name", 0, ""));
        }

        [Test]
        public void CreateReferenceGroupThrowsIfReferenceNameIsEmpty()
        {
            var sut = CreateSut();

            Assert.Throws<ArgumentException>(() => sut.CreateReferenceGroup("id", "", 0, "inner"));
        }

        private static IRegexGroupFactory CreateSut() => new RegexGroupService(null);
    }
}