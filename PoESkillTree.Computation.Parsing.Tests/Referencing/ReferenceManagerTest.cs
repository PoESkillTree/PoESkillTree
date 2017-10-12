using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class ReferenceManagerTest
    {
        [Test]
        public void ValidateThrowsIfReferencedMatchersContainsValues()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", "text # stuff"),
            };
            var sut = new ReferenceManager(referencedMatchersList, DefaultStatMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfReferencedMatchersContainsReferences()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", "text ({Matchers2}) stuff"),
                MockReferencedMatchers("Matchers2", "a"),
            };
            var sut = new ReferenceManager(referencedMatchersList, DefaultStatMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfReferencedMatchersNamesAreNotUnique()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", "b"),
                MockReferencedMatchers("Matchers1", "a"),
            };
            var sut = new ReferenceManager(referencedMatchersList, DefaultStatMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfReferenceNameIsUsedInReferencedMatchersAndStatMatchers()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("SMatchers1", "b"),
            };
            var sut = new ReferenceManager(referencedMatchersList, DefaultStatMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateDoesNotThrowIfStatMatchersNamesAreNotUnique()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }),
                MockStatMatchers(new[] { "SMatchers1" })
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.DoesNotThrow(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfStatMatchersWithReferenceNamesContainsValues()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "#")
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateDoesNotThrowIfStatMatchersWithoutReferenceNamesContainsValues()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new string[0], "#")
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.DoesNotThrow(() => sut.Validate());
        }

        [Test]
        public void ValidateDoesNotThrowIfStatMatchersContainReferences()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2})"),
                MockStatMatchers(new[] { "SMatchers2" }, "({Matchers1})"),
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.DoesNotThrow(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfStatMatchersContainCyclicalReferences()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2})"),
                MockStatMatchers(new[] { "SMatchers2" }, "({SMatchers1})")
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfStatMatchersContainCyclicalReferencesComplex()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2}) ({SMatchers2})"),
                MockStatMatchers(new[] { "SMatchers2" }, "({SMatchers5})"),
                MockStatMatchers(new[] { "SMatchers2" }, "({SMatchers3}) ({SMatchers4}) ({Matchers2})"),
                MockStatMatchers(new[] { "SMatchers3", "SMatchers4" }, "({Matchers2})"),
                MockStatMatchers(new[] { "SMatchers4" }, "({Matchers1}) ({SMatchers1})"),
                MockStatMatchers(new[] { "SMatchers5" }, "({Matchers1})"),
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

        [Test]
        public void ValidateThrowsIfStatMatcherContainsUnknownReferences()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2})")
            };
            var sut = new ReferenceManager(DefaultReferencedMatchersList, statMatchersList);

            Assert.Throws<ParseException>(() => sut.Validate());
        }

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

        private static readonly IReadOnlyList<IReferencedMatchers> DefaultReferencedMatchersList =
            new[]
            {
                MockReferencedMatchers("Matchers1", "a", "b"),
                MockReferencedMatchers("Matchers2", "1", "2", "3")
            };

        private static IReferencedMatchers MockReferencedMatchers(string referenceName,
            params string[] patterns)
        {
            var data = patterns.Select(p => new ReferencedMatcherData(p, null)).ToList();
            var mock = new Mock<IReferencedMatchers>();
            mock.SetupGet(m => m.ReferenceName).Returns(referenceName);
            mock.Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            return mock.Object;
        }

        private static readonly IReadOnlyList<IStatMatchers> DefaultStatMatchersList =
            new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "a", "aa", "aaa"),
                MockStatMatchers(new[] { "SMatchers2", "SMatchers1" }, "1"),
                MockStatMatchers(new[] { "SMatchers3" }, "q", "w"),
                MockStatMatchers(new string[0], "x"),
            };

        private static IStatMatchers MockStatMatchers(IReadOnlyList<string> referenceNames,
            params string[] patterns)
        {
            var data = patterns.Select(p => new MatcherData(p, null)).ToList();
            var mock = new Mock<IStatMatchers>();
            mock.SetupGet(m => m.ReferenceNames).Returns(referenceNames);
            mock.Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
            return mock.Object;
        }

    }
}