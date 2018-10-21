using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class DataValidatingTest : CompositionRootTestBase
    {
        [Test]
        public void ReferencesAreValid()
        {
            var parsingData = CompositionRoot.ParsingData;
            var referencedMatchers = parsingData.ReferencedMatchers;
            var statMatchers = parsingData.StatMatchers;

            Assert.DoesNotThrow(() => ReferenceValidator.Validate(referencedMatchers, statMatchers));
        }

        [Test]
        public void ReferencedMatchersHaveCorrectlyTypedData()
        {
            var parsingData = CompositionRoot.ParsingData;
            var referencedMatchers = parsingData.ReferencedMatchers;

            foreach (var matchers in referencedMatchers)
            {
                var type = matchers.MatchType;
                foreach (var data in matchers)
                {
                    Assert.IsInstanceOf(type, data.Match);
                }
            }
        }
    }
}
