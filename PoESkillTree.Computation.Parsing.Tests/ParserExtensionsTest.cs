using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ParserExtensionsTest
    {
        [Test]
        public void ParseSetsSourceWhereGlobal()
        {
            var originalSource = new ModifierSource.Local.Tree();
            var innerModifiers = new[]
            {
                Helper.MockModifier(source: new ModifierSource.Local.Given()),
                Helper.MockModifier(source: new ModifierSource.Global()),
            };
            var expectedModifiers = new[]
            {
                innerModifiers[0],
                new Modifier(innerModifiers[1].Stats, innerModifiers[1].Form, innerModifiers[1].Value, originalSource),
            };
            var statLine = "stat";
            var innerResult = new ParseResult(true, "", innerModifiers);
            var parser = Mock.Of<IParser>(p => p.Parse(statLine) == innerResult);

            var actual = parser.Parse(statLine, originalSource);

            Assert.AreEqual(expectedModifiers, actual.Result);
        }
    }
}