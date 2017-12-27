using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Referencing;
using static PoESkillTree.Computation.Parsing.Tests.Referencing.MatcherMocks;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    [TestFixture]
    public class ReferenceValidatorTest
    {
        [Test]
        public void ValidateThrowsIfReferencedMatchersContainsValues()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", "text # stuff"),
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(referencedMatchersList, DefaultStatMatchersList));
        }

        [Test]
        public void ValidateThrowsIfReferencedMatchersContainsReferences()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", "text ({Matchers2}) stuff"),
                MockReferencedMatchers("Matchers2", "a"),
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(referencedMatchersList, DefaultStatMatchersList));
        }

        [Test]
        public void ValidateThrowsIfReferencedMatchersNamesAreNotUnique()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", "b"),
                MockReferencedMatchers("Matchers1", "a"),
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(referencedMatchersList, DefaultStatMatchersList));
        }

        [Test]
        public void ValidateThrowsIfReferenceNameIsUsedInReferencedMatchersAndStatMatchers()
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("SMatchers1", "b"),
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(referencedMatchersList, DefaultStatMatchersList));
        }

        [Test]
        public void ValidateDoesNotThrowIfStatMatchersNamesAreNotUnique()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }),
                MockStatMatchers(new[] { "SMatchers1" })
            };

            Assert.DoesNotThrow(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [Test]
        public void ValidateThrowsIfStatMatchersWithReferenceNamesContainsValues()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "#")
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [Test]
        public void ValidateDoesNotThrowIfStatMatchersWithoutReferenceNamesContainsValues()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new string[0], "#")
            };

            Assert.DoesNotThrow(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [Test]
        public void ValidateDoesNotThrowIfStatMatchersContainReferences()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2})"),
                MockStatMatchers(new[] { "SMatchers2" }, "({Matchers1})"),
            };

            Assert.DoesNotThrow(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [Test]
        public void ValidateThrowsIfStatMatchersContainCyclicalReferences()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2})"),
                MockStatMatchers(new[] { "SMatchers2" }, "({SMatchers1})")
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
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

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [Test]
        public void ValidateThrowsIfStatMatcherContainsUnknownReferences()
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "({SMatchers2})")
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [TestCase("value")]
        [TestCase("value5_xyz")]
        [TestCase("reference")]
        [TestCase("reference_groupNameX")]
        public void ValidateThrowsIfStatMatcherContainsInvalidGroupNames(string groupName)
        {
            var statMatchersList = new[]
            {
                MockStatMatchers(new string[0], $"text (?<{groupName}>stuff)")
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(DefaultReferencedMatchersList, statMatchersList));
        }

        [TestCase("value")]
        [TestCase("value5_xyz")]
        [TestCase("reference")]
        [TestCase("reference_groupNameX")]
        public void ValidateThrowsIfReferencedMatcherContainsInvalidGroupNames(string groupName)
        {
            var referencedMatchersList = new[]
            {
                MockReferencedMatchers("Matchers1", $"text (?<{groupName}>stuff)")
            };

            Assert.Throws<ParseException>(() =>
                ReferenceValidator.Validate(referencedMatchersList, DefaultStatMatchersList));
        }
    }
}