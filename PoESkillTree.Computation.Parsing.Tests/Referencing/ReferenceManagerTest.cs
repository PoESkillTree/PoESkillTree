using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Referencing;
using static PoESkillTree.Computation.Parsing.Tests.Referencing.MatcherMocks;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class ReferenceManagerTest
    {
        [TestCase("Matchers1", ExpectedResult = true)]
        [TestCase("Matchers2", ExpectedResult = true)]
        [TestCase("Matchers3", ExpectedResult = false)]
        [TestCase("SMatchers1", ExpectedResult = true)]
        [TestCase("SMatchers2", ExpectedResult = true)]
        [TestCase("SMatchers3", ExpectedResult = true)]
        public bool ContainsReferenceReturnsCorrectResult(string referenceName)
        {
            var sut = new ReferenceManager(DefaultReferencedMatchersList, DefaultStatMatchersList);

            return sut.ContainsReference(referenceName);
        }

        [TestCase("Matchers1")]
        [TestCase("Matchers2")]
        public void GetReferencesReturnsCorrectResultForReferencedMatchersName(string referenceName)
        {
            var sut = new ReferenceManager(DefaultReferencedMatchersList, DefaultStatMatchersList);
            var expected = DefaultReferencedMatchersList
                .First(r => r.ReferenceName == referenceName)
                .Select(d => d.Regex);

            CollectionAssert.AreEqual(expected, sut.GetRegexes(referenceName));
        }

        [TestCase("SMatchers1")]
        [TestCase("SMatchers2")]
        [TestCase("SMatchers3")]
        public void GetReferencesReturnsCorrectResultForStatMatchersName(string referenceName)
        {
            var sut = new ReferenceManager(DefaultReferencedMatchersList, DefaultStatMatchersList);
            var expected = DefaultStatMatchersList
                .Where(r => r.ReferenceNames.Contains(referenceName))
                .SelectMany(r => r.Select(d => d.Regex));

            CollectionAssert.AreEqual(expected, sut.GetRegexes(referenceName));
        }

        [TestCase("Matchers3")]
        [TestCase("SMatchers4")]
        public void GetReferencesReturnsEmptyEnumerableIfReferenceNameIsUnknown(string referenceName)
        {
            var sut = new ReferenceManager(DefaultReferencedMatchersList, DefaultStatMatchersList);

            CollectionAssert.IsEmpty(sut.GetRegexes(referenceName));
        }

    }
}