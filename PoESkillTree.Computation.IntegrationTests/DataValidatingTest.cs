using NUnit.Framework;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class DataValidatingTest
    {
        [Test]
        public void ReferencesAreValid()
        {
            var compositionRoot = new CompositionRoot();
            var referencedMatchers = compositionRoot.ReferencedMatchers;
            var statMatchers = compositionRoot.StatMatchers;

            Assert.DoesNotThrow(() => ReferenceValidator.Validate(referencedMatchers, statMatchers));
        }
    }
}
