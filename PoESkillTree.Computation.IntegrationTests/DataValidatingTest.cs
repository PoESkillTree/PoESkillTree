using NUnit.Framework;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class DataValidatingTest
    {
        [Test]
        public void ReferencesAreValid()
        {
            var parsingData = new ParsingData(new BuilderFactories());
            var referencedMatchers = parsingData.ReferencedMatchers;
            var statMatchers = parsingData.StatMatchers;

            Assert.DoesNotThrow(() => ReferenceValidator.Validate(referencedMatchers, statMatchers));
        }
    }
}
