using NUnit.Framework;
using PoESkillTree.Computation.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Tests.Resolving
{
    [TestFixture]
    public class UnresolvedBuilderTest
    {
        [Test]
        public void BuildThrowsParseException()
        {
            var sut = CreateSut();

            Assert.Throws<UnresolvedException>(() => sut.Build(default));
        }

        [Test]
        public void ResolveUsesInjectedFunction()
        {
            var expected = 5;
            var sut = CreateSut(expected);

            var actual = sut.Resolve(BuildersHelper.MockResolveContext());

            Assert.AreEqual(expected, actual);
        }

        private static UnresolvedBuilder<int, string> CreateSut(int resolved = 0) => 
            new UnresolvedBuilder<int, string>("", _ => resolved);
    }
}