using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    // CompositeCoreStatBuilder is also tested from StatBuilderTest.
    [TestFixture]
    public class CompositeCoreStatBuilderTest
    {
        [Test]
        public void BuildWithMultipleResultEntryReturnsCorrectResults()
        {
            var multiResults = new[]
            {
                new StatBuilderResult(new IStat[0], ModifierSource, Funcs.Identity),
                new StatBuilderResult(new IStat[0], ModifierSource, Funcs.Identity),
            };
            var singleResults = new[]
            {
                new StatBuilderResult(new IStat[0], ModifierSource, Funcs.Identity),
            };
            var multiCoreStatBuilder = Mock.Of<ICoreStatBuilder>(b => b.Build(default, ModifierSource) == multiResults);
            var singleCoreStatBuilder =
                Mock.Of<ICoreStatBuilder>(b => b.Build(default, ModifierSource) == singleResults);
            var sut = new CompositeCoreStatBuilder(multiCoreStatBuilder, singleCoreStatBuilder);

            var results = sut.Build(default, ModifierSource).ToList();

            Assert.That(results, Has.Exactly(2).Items);
        }

        private static readonly ModifierSource ModifierSource = new ModifierSource.Global();
    }
}