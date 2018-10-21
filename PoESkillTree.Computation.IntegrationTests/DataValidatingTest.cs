using System.Threading.Tasks;
using NUnit.Framework;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class DataValidatingTest
    {
        [Test]
        public async Task ReferencesAreValid()
        {
            var parsingData = await new AsyncCompositionRoot().ParsingData.ConfigureAwait(false);
            var referencedMatchers = parsingData.ReferencedMatchers;
            var statMatchers = parsingData.StatMatchers;

            Assert.DoesNotThrow(() => ReferenceValidator.Validate(referencedMatchers, statMatchers));
        }

        [Test]
        public async Task ReferencedMatchersHaveCorrectlyTypedData()
        {
            var parsingData = await new AsyncCompositionRoot().ParsingData.ConfigureAwait(false);
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
