using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.Referencing;
using static PoESkillTree.Computation.Parsing.Referencing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class RegexGroupServiceParserTest
    {
        [Test]
        public void ParseValuesParsesAllGroups()
        {
            var groups = new Dictionary<string, string>
            {
                { ValueGroupPrefix + "0", "1" },
                { ValueGroupPrefix + "1", "2" },
                { ValueGroupPrefix + "2", "3" },
            };
            var expected = new[] { "1", "2", "3" };

            var actual = ParseValues(groups);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(ValueGroupPrefix + "Suffix")]
        [TestCase(ValueGroupPrefix + "PrefixSuffix", "Prefix")]
        public void ParseValuesReturnsCorrectSingleResult(string groupKey, string groupPrefix = "")
        {
            var groups = new Dictionary<string, string>
            {
                { groupKey, "2" },
            };
            var expected = new[] { "2" };

            var actual = ParseValues(groups, groupPrefix);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase(ValueGroupPrefix + "Suffix", "Prefix")]
        [TestCase(ValueGroupPrefix + "_Suffix")]
        [TestCase("PrefixSuffix", "Prefix")]
        [TestCase("Suffix")]
        public void ParseValuesReturnsCorrectEmptyResult(string groupKey, string groupPrefix = "")
        {
            var groups = new Dictionary<string, string>
            {
                { groupKey, "1" },
            };

            var actual = ParseValues(groups, groupPrefix);

            CollectionAssert.IsEmpty(actual);
        }

        [Test]
        public void ParseReferencesParsesAllGroups()
        {
            var groupNames = new[]
            {
                ReferenceGroupPrefix + "0_name0_0",
                ReferenceGroupPrefix + "1_name1_1",
                ReferenceGroupPrefix + "2_name2_2",
            };
            var expected = new[]
            {
                ("name0", 0, "0_"),
                ("name1", 1, "1_"),
                ("name2", 2, "2_")
            };

            var actual = ParseReferences(groupNames);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase("name0", 0, "0")]
        [TestCase("xyz", 100, "prefix")]
        [TestCase("name0", 0, "", "0")]
        [TestCase("name0", 0, "nestedPrefix", "prefix")]
        public void ParseReferencesReturnsCorrectSingleResult(
            string nestedReferenceName, int nestedMatcherIndex, string nestedPrefix, string groupPrefix = "")
        {
            var nestedGroupPrefix = groupPrefix + nestedPrefix + "_";
            var groupNames = new[]
            {
                ReferenceGroupPrefix + nestedGroupPrefix + nestedReferenceName + "_" + nestedMatcherIndex
            };
            var expected = new[]
            {
                (nestedReferenceName, nestedMatcherIndex, nestedGroupPrefix)
            };

            var actual = ParseReferences(groupNames, groupPrefix);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestCase("x" + ReferenceGroupPrefix + "0_name0_0")]
        [TestCase(ReferenceGroupPrefix + "0_0_name0_0")]
        [TestCase(ReferenceGroupPrefix + "name0_0")]
        [TestCase(ReferenceGroupPrefix + "0_name0_x")]
        [TestCase(ReferenceGroupPrefix + "0_name0_0", "1")]
        [TestCase(ReferenceGroupPrefix + "0_name0_0", "0_")]
        public void ParseReferencesReturnsCorrectEmptyResult(string groupName, string groupPrefix = "")
        {
            var groupNames = new[]
            {
                groupName
            };

            var actual = ParseReferences(groupNames, groupPrefix);

            CollectionAssert.IsEmpty(actual);
        }

        private static IRegexGroupParser CreateParser()
        {
            var valueBuildersMock = new Mock<IValueBuilders>();
            for (int i = 0; i < 4; i++)
            {
                var i1 = i;
                valueBuildersMock
                    .Setup(f => f.Create(i1))
                    .Returns(Mock.Of<IValueBuilder>(v => v.ToString() == i1.ToString()));
            }
            return new RegexGroupService(valueBuildersMock.Object);
        }

        private static string[] ParseValues(
            IReadOnlyDictionary<string, string> groups, string groupPrefix = "")
        {
            return CreateParser()
                .ParseValues(groups, groupPrefix)
                .Select(v => v.ToString())
                .ToArray();
        }

        private static (string referenceName, int matcherIndex, string groupPrefix)[] ParseReferences(
            IEnumerable<string> groupNames, string groupPrefix = "")
        {
            return CreateParser()
                .ParseReferences(groupNames, groupPrefix)
                .ToArray();
        }
    }
}