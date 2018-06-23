using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class EntityBuilderTest
    {
        [Test]
        public void ResolveReturnsSelf()
        {
            var sut = new EntityBuilder(default(Entity));

            var actual = sut.Resolve(null);

            Assert.AreEqual(sut, actual);
        }

        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character, Entity.Totem, Entity.Minion)]
        public void BuildReturnsConstructorArgument(params Entity[] expected)
        {
            var sut = new EntityBuilder(expected);

            var actual = sut.Build(default);

            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    public class ModifierSourceEntityBuilderTest
    {
        [Test]
        public void ResolveReturnsSelf()
        {
            var sut = new ModifierSourceEntityBuilder();

            var actual = sut.Resolve(null);

            Assert.AreEqual(sut, actual);
        }

        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character)]
        public void BuildReturnsArgument(Entity expected)
        {
            var sut = new ModifierSourceEntityBuilder();

            var actual = sut.Build(expected);

            Assert.AreEqual(expected, actual.Single());
        }
    }
}