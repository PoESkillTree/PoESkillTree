using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Referencing;
using static PoESkillTree.Computation.Parsing.Tests.Referencing.MatcherMocks;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class ReferenceServiceTest
    {
        [TestCase("Matchers1")]
        [TestCase("Matchers2")]
        public void GetReferencesReturnsCorrectResultForReferencedMatchersName(string referenceName)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);
            var expected = DefaultReferencedMatchersList
                .First(r => r.ReferenceName == referenceName).Data
                .Select(d => d.Regex);

            CollectionAssert.AreEqual(expected, sut.GetRegexes(referenceName));
        }

        [TestCase("SMatchers1")]
        [TestCase("SMatchers2")]
        [TestCase("SMatchers3")]
        public void GetReferencesReturnsCorrectResultForStatMatchersName(string referenceName)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);
            var expected = DefaultStatMatchersList
                .Where(r => r.ReferenceNames.Contains(referenceName))
                .SelectMany(r => r.Data.Select(d => d.Regex));

            CollectionAssert.AreEqual(expected, sut.GetRegexes(referenceName));
        }

        [TestCase("Matchers3")]
        [TestCase("SMatchers4")]
        public void GetReferencesReturnsEmptyEnumerableIfReferenceNameIsUnknown(string referenceName)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);

            CollectionAssert.IsEmpty(sut.GetRegexes(referenceName));
        }

        [TestCase("Matchers1", 0, ExpectedResult = "a")]
        [TestCase("Matchers1", 2, ExpectedResult = null)]
        [TestCase("Matchers2", 2, ExpectedResult = "3")]
        [TestCase("SMatchers1", 1, ExpectedResult = null)]
        public string TryGetReferencedMatcherDataOutputsCorrectMatcherData(string referenceName, int matcherIndex)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);

            sut.TryGetReferencedMatcherData(referenceName, matcherIndex, out var matcherData);
            return matcherData?.Regex;
        }

        [TestCase("Matchers1", 0, ExpectedResult = true)]
        [TestCase("Matchers1", 2, ExpectedResult = false)]
        [TestCase("Matchers2", 2, ExpectedResult = true)]
        [TestCase("SMatchers1", 1, ExpectedResult = false)]
        public bool TryGetReferencedMatcherDataReturnsCorrectResult(string referenceName, int matcherIndex)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);

            return sut.TryGetReferencedMatcherData(referenceName, matcherIndex, out var _);
        }

        [TestCase("SMatchers1", 0, ExpectedResult = "a")]
        [TestCase("SMatchers1", 3, ExpectedResult = "1")]
        [TestCase("SMatchers1", 4, ExpectedResult = null)]
        [TestCase("SMatchers2", 0, ExpectedResult = "1")]
        [TestCase("Matchers1", 1, ExpectedResult = null)]
        public string TryGetMatcherDataOutputsCorrectMatcherData(string referenceName, int matcherIndex)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);

            sut.TryGetMatcherData(referenceName, matcherIndex, out var matcherData);
            return matcherData?.Regex;
        }

        [TestCase("SMatchers1", 0, ExpectedResult = true)]
        [TestCase("SMatchers1", 3, ExpectedResult = true)]
        [TestCase("SMatchers1", 4, ExpectedResult = false)]
        [TestCase("SMatchers2", 0, ExpectedResult = true)]
        [TestCase("Matchers1", 1, ExpectedResult = false)]
        public bool TryGetMatcherDataReturnsCorrectResult(string referenceName, int matcherIndex)
        {
            var sut = new ReferenceService(DefaultReferencedMatchersList, DefaultStatMatchersList);

            return sut.TryGetMatcherData(referenceName, matcherIndex, out var _);
        }

    }
}